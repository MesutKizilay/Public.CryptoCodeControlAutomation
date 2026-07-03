using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.CodeAdjustmentLogs.Queries.GetList
{
    public class GetListCodeAdjustmentLogDto
    {
        public long CodeAdjustmentLogId { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public string? SalesOrderNo { get; set; }
        public string? SalesItemNo { get; set; }
        public string? PlannedOrderNo { get; set; }
        public CodeStatus? FromStatus { get; set; }
        public CodeStatus? ToStatus { get; set; }
        public DateTime? FromShiftDate { get; set; }
        public DateTime? ToShiftDate { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
