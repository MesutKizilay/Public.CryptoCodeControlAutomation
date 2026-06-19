using System;
using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Domain.Entities
{
    public class Code : IEntity
    {
        public long CodeId { get; set; }
        public string CodeValue { get; set; }
        public long SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }
        public int? StationId { get; set; }
        public byte? PackagingLevel { get; set; }
        public CodeStatus Status { get; set; }
        public DateTime? AllocatedAt { get; set; }
        public DateTime? ProducedAt { get; set; }
        public DateTime? ShiftDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? RecoverAt { get; set; }
    }
}
