namespace CryptoCodeControlAutomation.Application.Features.DataMatrixPrints.Commands.GeneratePdf
{
    public class GenerateDataMatrixPdfResponse
    {
        public byte[] Content { get; set; } = [];
        public string ContentType { get; set; } = "application/pdf";
        public string FileName { get; set; } = string.Empty;
        public int CodeCount { get; set; }
    }
}
