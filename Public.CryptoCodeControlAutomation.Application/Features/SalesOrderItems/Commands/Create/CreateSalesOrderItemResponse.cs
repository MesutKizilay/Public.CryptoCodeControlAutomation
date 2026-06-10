namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Create
{
    public class CreateSalesOrderItemResponse
    {
        public long SalesOrderItemId { get; set; }
        public long? UploadJobId { get; set; }
    }
}