using System;
using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Domain.Entities
{
    public class PlannedOrder : IEntity
    {
        public long PlannedOrderId { get; set; }
        public string PlannedOrderNo { get; set; }
        public string MaterialNo { get; set; }
        public string LineCode { get; set; }
        public int? TotalCaseQty { get; set; }
        public int TotalUnitQty { get; set; }
        public bool P1Enabled { get; set; }
        public bool P2Enabled { get; set; }
        public bool P3Enabled { get; set; }
        public bool P4Enabled { get; set; }
        public PlannedOrderStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
