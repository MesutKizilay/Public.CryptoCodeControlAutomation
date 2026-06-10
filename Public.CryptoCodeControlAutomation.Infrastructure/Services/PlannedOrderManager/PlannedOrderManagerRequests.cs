using System.Text.Json.Serialization;

namespace CryptoCodeControlAutomation.Infrastructure.Services.PlannedOrderManagerService
{
    internal sealed class PlannedOrderRequest
    {
        [JsonPropertyName("IV_VENUM")]
        public string? IV_VENUM { get; set; }
    }
}
