using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Domain.Entities
{
    public class CodeAdjustmentLogItem : IEntity
    {
        public long CodeAdjustmentLogItemId { get; set; }
        public long CodeAdjustmentLogId { get; set; }
        public long CodeId { get; set; }
        public string CodeValue { get; set; } = string.Empty;
        public CodeStatus OldStatus { get; set; }
        public CodeStatus NewStatus { get; set; }
        public DateTime? OldShiftDate { get; set; }
        public DateTime? NewShiftDate { get; set; }
        public DateTime? OldProducedAt { get; set; }
        public DateTime? NewProducedAt { get; set; }
        public DateTime? OldExpirationDate { get; set; }
        public DateTime? NewExpirationDate { get; set; }
        public CodeAdjustmentLog CodeAdjustmentLog { get; set; } = null!;
    }
}
