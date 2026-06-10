using System.Text.Json.Serialization;

namespace CryptoCodeControlAutomation.Infrastructure.Services.SalesOrderItemManagerService
{
    internal sealed class SalesOrderItemRequest
    {
        [JsonPropertyName("IS_SIPARIS")]
        public SalesOrderItemRequestItem? IS_SIPARIS { get; set; }
    }

    internal sealed class SalesOrderItemRequestItem
    {
        [JsonPropertyName("VBELN")]
        public string? VBELN { get; set; }

        [JsonPropertyName("POSNR")]
        public string? POSNR { get; set; }
    }
}
