using System.Text.Json.Serialization;

namespace CryptoCodeControlAutomation.Infrastructure.Services.SalesOrderItemManagerService
{
    internal sealed class SalesOrderItemResponse
    {
        [JsonPropertyName("ES_BAPIRET2")]
        public SalesOrderItemBapiReturn? ES_BAPIRET2 { get; set; }

        [JsonPropertyName("ES_SIPARIS")]
        public Siparis? ES_SIPARIS { get; set; }
    }

    internal sealed class SalesOrderItemBapiReturn
    {
        [JsonPropertyName("TYPE")]
        public string? TYPE { get; set; }

        [JsonPropertyName("MESSAGE")]
        public string? MESSAGE { get; set; }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    internal sealed class Siparis
    {
        [JsonPropertyName("MATNR")]
        public string? MATNR { get; set; }

        [JsonPropertyName("KWMENG")]
        public string? KWMENG { get; set; }

        [JsonPropertyName("KWMENG_ADT")]
        public string? KWMENG_ADT { get; set; }

        [JsonPropertyName("GTIN")]
        public object? GTIN { get; set; }

        [JsonPropertyName("APIKEY")]
        public string? APIKEY { get; set; }
    }

    internal sealed class ApiErrorResponse
    {
        [JsonPropertyName("error")]
        public ApiError? Error { get; set; }
    }

    internal sealed class ApiError
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
