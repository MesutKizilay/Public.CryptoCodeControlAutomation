namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetSalesPlannedOrderSummary
{
    public class GetSalesPlannedOrderSummaryDto
    {
        public long SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }
        public string SalesOrderNo { get; set; } = string.Empty;
        public string SalesItemNo { get; set; } = string.Empty;
        public int? SapCaseQty { get; set; }
        public int SapPlannedUnitQty { get; set; }
        public bool IsCodeUploaded { get; set; }
        public string? PlannedOrderNo { get; set; }
        public int? PlannedOrderUnitQty { get; set; }
    }
}
