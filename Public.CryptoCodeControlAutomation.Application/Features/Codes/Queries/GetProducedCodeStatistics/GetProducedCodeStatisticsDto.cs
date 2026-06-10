using System;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetProducedCodeStatistics
{
    public class GetProducedCodeStatisticsDto
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime PeriodStart { get; set; }
    }
}
