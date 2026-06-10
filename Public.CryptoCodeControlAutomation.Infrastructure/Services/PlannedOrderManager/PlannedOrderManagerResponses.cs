using System.Text.Json.Serialization;

namespace CryptoCodeControlAutomation.Infrastructure.Services.PlannedOrderManagerService
{
    internal sealed class PlannedOrderResponse
    {
        [JsonPropertyName("ES_BAPIRET2")]
        public PlannedOrderBapiReturn? ES_BAPIRET2 { get; set; }

        [JsonPropertyName("EV_PLNUM")]
        public string? EV_PLNUM { get; set; }
    }

    internal sealed class PlannedOrderBapiReturn
    {
        [JsonPropertyName("TYPE")]
        public string? TYPE { get; set; }

        [JsonPropertyName("MESSAGE")]
        public string? MESSAGE { get; set; }
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
