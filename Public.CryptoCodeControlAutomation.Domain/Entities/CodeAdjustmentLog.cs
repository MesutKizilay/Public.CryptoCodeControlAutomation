using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Domain.Entities
{
    public class CodeAdjustmentLog : IEntity
    {
        public long CodeAdjustmentLogId { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public long? SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }
        public CodeStatus? FromStatus { get; set; }
        public CodeStatus? ToStatus { get; set; }
        public DateTime? FromShiftDate { get; set; }
        public DateTime? ToShiftDate { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<CodeAdjustmentLogItem> Items { get; set; } = new List<CodeAdjustmentLogItem>();
    }
}
