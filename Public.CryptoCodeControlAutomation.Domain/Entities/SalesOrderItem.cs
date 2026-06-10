using System;
using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Domain.Entities
{
    public class SalesOrderItem : IEntity
    {
        public long SalesOrderItemId { get; set; }
        public string SalesOrderNo { get; set; }
        public string SalesItemNo { get; set; }
        public string MaterialNo { get; set; }
        public string? GTIN { get; set; }
        public int? SapCaseQty { get; set; }
        public int SapPlannedUnitQty { get; set; }
        public int RemainingUnitQty { get; set; }
        public bool IsOpen { get; set; } = true;
        public SalesOrderItemStatus Status { get; set; }
        public SalesOrderItemApprovalStatus ApprovalStatus { get; set; } = SalesOrderItemApprovalStatus.PendingApproval;
        public string? ProductionApprovedByUsername { get; set; }
        public DateTime? ProductionApprovedAt { get; set; }
        public string? ShipmentApprovedByUsername { get; set; }
        public DateTime? ShipmentApprovedAt { get; set; }
        public DateTime? SapValidatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
