using CryptoCodeControlAutomation.Application.Features.PlannedOrders.Queries.GetPlannedOrderByPalletNumber;
using CryptoCodeControlAutomation.Application.Services.Validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace CryptoCodeControlAutomation.Infrastructure.Services.PlannedOrderManagerService
{
    public class PlannedOrderManager : IPlannedOrderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PlannedOrderManager> _logger;

        public PlannedOrderManager(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PlannedOrderManager> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GetPlannedOrderByPalletNumberDto> GetPlannedOrderByPalletNumber(string tbNo, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tbNo))
            {
                return new GetPlannedOrderByPalletNumberDto
                {
                    Success = false,
                    Message = "TB no zorunludur."
                };
            }

            var endpoint = _configuration["SalesOrderItemApi:PlannedOrderEndpoint"] ?? "RESTAdapter/Teknosin/PlannedOrder";
            var baseUrl = _configuration["SalesOrderItemApi:BaseUrl"];
            var endpointIsAbsolute = Uri.TryCreate(endpoint, UriKind.Absolute, out _);
            if (!endpointIsAbsolute && string.IsNullOrWhiteSpace(baseUrl))
            {
                return new GetPlannedOrderByPalletNumberDto
                {
                    Success = false,
                    Message = "SalesOrderItemApi:BaseUrl veya PlannedOrderEndpoint tanimli degil."
                };
            }

            var client = _httpClientFactory.CreateClient("SalesOrderItemApi");
            var payload = new PlannedOrderRequest
            {
                IV_VENUM = tbNo
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint)
            {
                Content = JsonContent.Create(payload)
            };

            try
            {
                using var response = await client.SendAsync(request, cancellationToken);
                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = TryDeserialize<ApiErrorResponse>(json);
                    return new GetPlannedOrderByPalletNumberDto
                    {
                        Success = false,
                        Message = error?.Error?.Message ?? $"Servis hatasi: {(int)response.StatusCode}"
                    };
                }

                var data = TryDeserialize<PlannedOrderResponse>(json);
                if (data == null)
                {
                    return new GetPlannedOrderByPalletNumberDto
                    {
                        Success = false,
                        Message = "Servisten gecersiz yanit alindi."
                    };
                }

                var type = data.ES_BAPIRET2?.TYPE?.Trim();
                if (!string.IsNullOrEmpty(type) && !string.Equals(type, "S", StringComparison.OrdinalIgnoreCase))
                {
                    return new GetPlannedOrderByPalletNumberDto
                    {
                        Success = false,
                        Message = data.ES_BAPIRET2?.MESSAGE ?? "Servis hata dondu."
                    };
                }

                return new GetPlannedOrderByPalletNumberDto
                {
                    Success = true,
                    PlannedOrderNo = data.EV_PLNUM,
                    Message = data.ES_BAPIRET2?.MESSAGE
                };
            }
            catch (Exception ex)
            {
                //return new GetPlannedOrderByPalletNumberDto
                //{
                //    Success = true,
                //    PlannedOrderNo = "a",
                //    Message = "tmm tmm"
                //};
                _logger.LogError(ex, "PlannedOrder validation failed.");
                return new GetPlannedOrderByPalletNumberDto
                {
                    Success = false,
                    Message = $"Servise erisilemedi: {ex.Message}"
                };
            }
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
