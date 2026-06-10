using System;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.ValidateSalesOrderItem
{
    public class ValidateSalesOrderItemDto
    {
        public bool Success { get; set; }
        public string? MaterialNo { get; set; }
        public string? GTIN { get; set; }
        public int? PlannedUnitQty { get; set; }
        public int? CaseQty { get; set; }
        public DateTime? SapValidatedAt { get; set; }
        public string? Message { get; set; }
    }
}