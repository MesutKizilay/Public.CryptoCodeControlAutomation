namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustShiftDate
{
    public class AdjustCodeShiftDateResponse
    {
        public int UpdatedCount { get; set; }
        public long CodeAdjustmentLogId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
