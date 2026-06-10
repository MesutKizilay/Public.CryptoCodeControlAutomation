using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetProducedCodeStatistics
{
    public class GetProducedCodeStatisticsQuery : IRequest<List<GetProducedCodeStatisticsDto>>
    {
        public long? SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }
        public string Period { get; set; } = "monthly";

        public class GetProducedCodeStatisticsQueryHandler : IRequestHandler<GetProducedCodeStatisticsQuery, List<GetProducedCodeStatisticsDto>>
        {
            private readonly ICodeRepository _codeRepository;

            public GetProducedCodeStatisticsQueryHandler(ICodeRepository codeRepository)
            {
                _codeRepository = codeRepository;
            }

            public async Task<List<GetProducedCodeStatisticsDto>> Handle(GetProducedCodeStatisticsQuery request, CancellationToken cancellationToken)
            {
                var period = NormalizePeriod(request.Period);
                var endDate = DateTime.Now.Date;
                var startDate = GetStartDate(period, endDate);

                var query = _codeRepository.Query()
                    .Where(c => c.Status == CodeStatus.ProducedOk && c.ProducedAt.HasValue);

                if (request.SalesOrderItemId.HasValue)
                {
                    query = query.Where(c => c.SalesOrderItemId == request.SalesOrderItemId.Value);
                }

                if (request.PlannedOrderId.HasValue)
                {
                    query = query.Where(c => c.PlannedOrderId == request.PlannedOrderId.Value);
                }

                var dailyData = await query
                    .Where(c => c.ProducedAt!.Value.Date >= startDate && c.ProducedAt!.Value.Date <= endDate)
                    .GroupBy(c => c.ProducedAt!.Value.Date)
                    .Select(g => new DailyCount { Date = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                return BuildBuckets(period, startDate, endDate, dailyData);
            }

            private static string NormalizePeriod(string? period)
            {
                if (string.IsNullOrWhiteSpace(period))
                {
                    return "monthly";
                }

                return period.Trim().ToLowerInvariant() switch
                {
                    "daily" => "daily",
                    "weekly" => "weekly",
                    "monthly" => "monthly",
                    "yearly" => "yearly",
                    _ => "monthly"
                };
            }

            private static DateTime GetStartDate(string period, DateTime endDate)
            {
                return period switch
                {
                    "daily" => endDate.AddDays(-29),
                    "weekly" => endDate.AddDays(-7 * 11),
                    "yearly" => new DateTime(endDate.Year - 4, 1, 1),
                    _ => new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-11)
                };
            }

            private static List<GetProducedCodeStatisticsDto> BuildBuckets(
                string period,
                DateTime startDate,
                DateTime endDate,
                List<DailyCount> dailyData)
            {
                var result = new List<GetProducedCodeStatisticsDto>();
                var dailyCounts = dailyData.ToDictionary(x => x.Date, x => x.Count);

                if (period == "daily")
                {
                    for (var cursor = startDate.Date; cursor <= endDate.Date; cursor = cursor.AddDays(1))
                    {
                        dailyCounts.TryGetValue(cursor, out var count);
                        result.Add(new GetProducedCodeStatisticsDto
                        {
                            PeriodStart = cursor,
                            Label = cursor.ToString("dd.MM"),
                            Count = count
                        });
                    }

                    return result;
                }

                if (period == "weekly")
                {
                    var weeklyCounts = new Dictionary<DateTime, int>();
                    foreach (var entry in dailyCounts)
                    {
                        var weekStart = StartOfWeek(entry.Key, DayOfWeek.Monday);
                        if (weeklyCounts.ContainsKey(weekStart))
                        {
                            weeklyCounts[weekStart] += entry.Value;
                        }
                        else
                        {
                            weeklyCounts[weekStart] = entry.Value;
                        }
                    }

                    var cursor = StartOfWeek(startDate, DayOfWeek.Monday);
                    var lastWeek = StartOfWeek(endDate, DayOfWeek.Monday);
                    for (; cursor <= lastWeek; cursor = cursor.AddDays(7))
                    {
                        weeklyCounts.TryGetValue(cursor, out var count);
                        result.Add(new GetProducedCodeStatisticsDto
                        {
                            PeriodStart = cursor,
                            Label = cursor.ToString("dd.MM"),
                            Count = count
                        });
                    }

                    return result;
                }

                if (period == "yearly")
                {
                    var yearlyCounts = new Dictionary<int, int>();
                    foreach (var entry in dailyCounts)
                    {
                        var year = entry.Key.Year;
                        if (yearlyCounts.ContainsKey(year))
                        {
                            yearlyCounts[year] += entry.Value;
                        }
                        else
                        {
                            yearlyCounts[year] = entry.Value;
                        }
                    }

                    for (var year = startDate.Year; year <= endDate.Year; year++)
                    {
                        yearlyCounts.TryGetValue(year, out var count);
                        result.Add(new GetProducedCodeStatisticsDto
                        {
                            PeriodStart = new DateTime(year, 1, 1),
                            Label = year.ToString(),
                            Count = count
                        });
                    }

                    return result;
                }

                var monthlyCounts = new Dictionary<DateTime, int>();
                foreach (var entry in dailyCounts)
                {
                    var monthStart = new DateTime(entry.Key.Year, entry.Key.Month, 1);
                    if (monthlyCounts.ContainsKey(monthStart))
                    {
                        monthlyCounts[monthStart] += entry.Value;
                    }
                    else
                    {
                        monthlyCounts[monthStart] = entry.Value;
                    }
                }

                var monthCursor = new DateTime(startDate.Year, startDate.Month, 1);
                var lastMonth = new DateTime(endDate.Year, endDate.Month, 1);
                for (; monthCursor <= lastMonth; monthCursor = monthCursor.AddMonths(1))
                {
                    monthlyCounts.TryGetValue(monthCursor, out var count);
                    result.Add(new GetProducedCodeStatisticsDto
                    {
                        PeriodStart = monthCursor,
                        Label = monthCursor.ToString("MM.yyyy"),
                        Count = count
                    });
                }

                return result;
            }

            private static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
            {
                var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
                return date.Date.AddDays(-diff);
            }

            private sealed class DailyCount
            {
                public DateTime Date { get; set; }
                public int Count { get; set; }
            }
        }
    }
}
