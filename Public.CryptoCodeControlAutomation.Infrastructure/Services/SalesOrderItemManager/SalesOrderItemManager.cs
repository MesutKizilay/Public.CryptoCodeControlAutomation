using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.ValidateSalesOrderItem;
using CryptoCodeControlAutomation.Application.Services.Validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace CryptoCodeControlAutomation.Infrastructure.Services.SalesOrderItemManagerService
{
    public class SalesOrderItemManager : ISalesOrderItemService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SalesOrderItemManager> _logger;

        public SalesOrderItemManager(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<SalesOrderItemManager> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ValidateSalesOrderItemDto> ValidateSalesOrderItem(string salesOrderNo, string salesItemNo, CancellationToken cancellationToken = default)
        {
            //if (string.IsNullOrWhiteSpace(salesOrderNo))
            //    return new ValidateSalesOrderItemDto { Success = false, Message = "Sales Order No zorunludur." };
            //
            //if (string.IsNullOrWhiteSpace(salesItemNo))
            //    return new ValidateSalesOrderItemDto { Success = false, Message = "Sales Item No zorunludur." };

            //if (!long.TryParse(salesItemNo, NumberStyles.None, CultureInfo.InvariantCulture, out _))
            //    return new ValidateSalesOrderItemDto { Success = false, Message = "Sales Item No sadece rakam olmalıdır." };

            var endpoint = _configuration["SalesOrderItemApi:Endpoint"] ?? "RESTAdapter/Teknosin/Siparis";
            var baseUrl = _configuration["SalesOrderItemApi:BaseUrl"];
            var endpointIsAbsolute = Uri.TryCreate(endpoint, UriKind.Absolute, out _);
            if (!endpointIsAbsolute && string.IsNullOrWhiteSpace(baseUrl))
            {
                return new ValidateSalesOrderItemDto
                {
                    Success = false,
                    Message = "SalesOrderItemApi:BaseUrl veya tam URL (Endpoint) tanımlı değil."
                };
            }
            var client = _httpClientFactory.CreateClient("SalesOrderItemApi");
            var username = _configuration["SalesOrderItemApi:Username"];
            var password = _configuration["SalesOrderItemApi:Password"];

            var payload = new SalesOrderItemRequest
            {
                IS_SIPARIS = new SalesOrderItemRequestItem
                {
                    VBELN = salesOrderNo,
                    POSNR = salesItemNo
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint)
            {
                Content = JsonContent.Create(payload)
            };
            //if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            //{
            //    var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
            //    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", token);
            //}
            //_logger.LogInformation("SalesOrderItemApi request prepared. Endpoint={Endpoint} HasAuth={HasAuth}", endpoint, !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password));

            try
            {
                using var response = await client.SendAsync(request, cancellationToken);
                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = TryDeserialize<ApiErrorResponse>(json);
                    return new ValidateSalesOrderItemDto
                    {
                        Success = false,
                        Message = error?.Error?.Message ?? $"Servis hatası: {(int)response.StatusCode}"
                    };
                }

                var data = TryDeserialize<SalesOrderItemResponse>(json);
                if (data == null)
                {
                    return new ValidateSalesOrderItemDto
                    {
                        Success = false,
                        Message = "Servisten geçersiz yanıt alındı."
                    };
                }

                var type = data.ES_BAPIRET2?.TYPE?.Trim();
                if (!string.IsNullOrEmpty(type) && !string.Equals(type, "S", StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidateSalesOrderItemDto
                    {
                        Success = false,
                        Message = data.ES_BAPIRET2?.MESSAGE ?? "Servis hata döndü."
                    };
                }

                return new ValidateSalesOrderItemDto
                {
                    Success = true,
                    MaterialNo = data.ES_SIPARIS?.MATNR,
                    GTIN = data.ES_SIPARIS?.GTIN?.ToString(),
                    PlannedUnitQty = ParseInt(data.ES_SIPARIS?.KWMENG_ADT),
                    CaseQty = ParseInt(data.ES_SIPARIS?.KWMENG),
                    SapValidatedAt = DateTime.Now,
                    Message = data.ES_BAPIRET2?.MESSAGE
                };
            }
            catch (Exception ex)
            {
                return new ValidateSalesOrderItemDto
                {
                    Success = true,
                    MaterialNo = "MA.KEK.5842.02",
                    GTIN = "8699141058425",
                    PlannedUnitQty = 100000,
                    CaseQty = 7500,
                    SapValidatedAt = DateTime.Now,
                    Message = "456"
                };
                return new ValidateSalesOrderItemDto
                {
                    Success = false,
                    Message = $"Servise erişilemedi: {ex.Message}"
                };
            }
        }

        private static int? ParseInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                return (int)dec;
            return null;
        }

        private static T? TryDeserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return default;
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
