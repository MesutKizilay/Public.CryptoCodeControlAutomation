using System;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeReportList
{
    public class GetCodeReportListDto
    {
        public string CodeValue { get; set; } = string.Empty;
        public CodeStatus Status { get; set; }
        public string SalesOrderNo { get; set; } = string.Empty;
        public string SalesItemNo { get; set; } = string.Empty;
        public string PlannedOrderNo { get; set; } = string.Empty;
        public DateTime? ProducedAt { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? RecoverAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
