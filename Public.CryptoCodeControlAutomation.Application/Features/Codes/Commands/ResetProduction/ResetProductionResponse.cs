namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.ResetProduction
{
    public class ResetProductionResponse
    {
        public int UpdatedCount { get; set; }
        public long CodeAdjustmentLogId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
