namespace CryptoCodeControlAutomation.Application.Features.PlannedOrders.Queries.GetPlannedOrderByPalletNumber
{
    public class GetPlannedOrderByPalletNumberDto
    {
        public bool Success { get; set; }
        public string? PlannedOrderNo { get; set; }
        public string? Message { get; set; }
    }
}
