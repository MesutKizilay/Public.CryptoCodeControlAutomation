namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.ExportCodeReport
{
    public class ExportCodeReportDto
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "text/csv";
        public string FileName { get; set; } = "codereport.csv";
    }
}