using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeLookup
{
    public class GetCodeLookupDto
    {
        public string Code { get; set; } = string.Empty;
        public CodeStatus Status { get; set; }
        public byte? PackagingLevel { get; set; }
        public int? StationId { get; set; }
        public string? StationCode { get; set; }
        public string? LineCode { get; set; }
        public long? PlannedOrderId { get; set; }
        public string? PlannedOrderNo { get; set; }
        public string? MaterialNo { get; set; }
        public string? PlannedOrderLine { get; set; }
        public PlannedOrderStatus? PlannedOrderStatus { get; set; }
        public long SalesOrderItemId { get; set; }
        public string SalesOrderNo { get; set; } = string.Empty;
        public string SalesItemNo { get; set; } = string.Empty;
        public string SalesMaterialNo { get; set; } = string.Empty;
        public string? GTIN { get; set; }
        public DateTime? AllocatedAt { get; set; }
        public DateTime? ProducedAt { get; set; }
    }
}
