namespace CryptoCodeControlAutomation.Application.Features.UploadJobs.Queries.GetBySalesOrderItemId
{
    public class GetUploadJobsBySalesOrderItemIdQueryDto
    {
        public long UploadJobId { get; set; }
        public long SalesOrderItemId { get; set; }
        public string? FilePath { get; set; }
        public byte Status { get; set; }
        public int? TotalRows { get; set; }
        public int? InsertedRows { get; set; }
        public string? ErrorText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
