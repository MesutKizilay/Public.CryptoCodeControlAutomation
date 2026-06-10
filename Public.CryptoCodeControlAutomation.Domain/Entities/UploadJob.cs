using System;
using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Domain.Entities
{
    public class UploadJob : IEntity
    {
        public long UploadJobId { get; set; }
        public long SalesOrderItemId { get; set; }
        public string FilePath { get; set; }
        public UploadJobStatus Status { get; set; }
        public int? TotalRows { get; set; }
        public int? InsertedRows { get; set; }
        public string ErrorText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
