using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.PlannedOrders.Queries.GetListBySalesOrderItemId
{
    public class GetListPlannedOrdersBySalesOrderItemIdDto
    {
        public long PlannedOrderId { get; set; }
        public string PlannedOrderNo { get; set; }
        public string MaterialNo { get; set; }
        public string LineCode { get; set; }
        public int TotalUnitQty { get; set; }
        public PlannedOrderStatus Status { get; set; }
        public int CodeCount { get; set; }
        public long SalesOrderItemId { get; internal set; }
    }
}
