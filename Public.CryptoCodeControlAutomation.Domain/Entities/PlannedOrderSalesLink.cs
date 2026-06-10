using Core.Persistence.Repositories;

namespace CryptoCodeControlAutomation.Domain.Entities
{
    public class PlannedOrderSalesLink : IEntity
    {
        public long LinkId { get; set; }
        public long PlannedOrderId { get; set; }
        public long SalesOrderItemId { get; set; }
        public int ReservedUnitQty { get; set; }
        public int ConsumedUnitQty { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
