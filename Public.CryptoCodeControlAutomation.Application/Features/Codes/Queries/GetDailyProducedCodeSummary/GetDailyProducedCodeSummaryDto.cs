namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetDailyProducedCodeSummary
{
    public class GetDailyProducedCodeSummaryDto
    {
        public DateTime ShiftDate { get; set; }
        public int ProducedCount { get; set; }
    }
}
