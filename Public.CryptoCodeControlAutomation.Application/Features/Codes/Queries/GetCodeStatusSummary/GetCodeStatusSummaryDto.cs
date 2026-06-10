namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeStatusSummary
{
    public class GetCodeStatusSummaryDto
    {
        public int Total { get; set; }
        public int Available { get; set; }
        public int Allocated { get; set; }
        public int ProducedOk { get; set; }
        public int Reject { get; set; }
        public int Scrap { get; set; }
        public int Void { get; set; }
    }
}
