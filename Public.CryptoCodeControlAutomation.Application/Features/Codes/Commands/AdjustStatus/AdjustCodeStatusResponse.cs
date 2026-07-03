namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustStatus
{
    public class AdjustCodeStatusResponse
    {
        public int UpdatedCount { get; set; }
        public long CodeAdjustmentLogId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
