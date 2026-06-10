using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeReportList;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.ExportCodeReport
{
    public partial class ExportCodeReportQuery : IRequest<ExportCodeReportDto>
    {
        public long? SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }
        public CodeStatus? Status { get; set; }
        public bool OnlyCodes { get; set; }

        public class ExportCodeReportQueryHandler : IRequestHandler<ExportCodeReportQuery, ExportCodeReportDto>
        {
            private readonly ICodeRepository _codeRepository;
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly IPlannedOrderRepository _plannedOrderRepository;

            public ExportCodeReportQueryHandler(ICodeRepository codeRepository, ISalesOrderItemRepository salesOrderItemRepository, IPlannedOrderRepository plannedOrderRepository)
            {
                _codeRepository = codeRepository;
                _salesOrderItemRepository = salesOrderItemRepository;
                _plannedOrderRepository = plannedOrderRepository;
            }

            public async Task<ExportCodeReportDto> Handle(ExportCodeReportQuery request, CancellationToken cancellationToken)
            {
                var codes = _codeRepository.Query();
                var sales = _salesOrderItemRepository.Query();
                var planned = _plannedOrderRepository.Query();

                var codeQuery = codes.AsQueryable();

                if (request.SalesOrderItemId.HasValue && request.SalesOrderItemId.Value > 0)
                {
                    codeQuery = codeQuery.Where(c => c.SalesOrderItemId == request.SalesOrderItemId.Value);
                }

                if (request.PlannedOrderId.HasValue && request.PlannedOrderId.Value > 0)
                {
                    codeQuery = codeQuery.Where(c => c.PlannedOrderId == request.PlannedOrderId.Value);
                }

                if (request.Status.HasValue)
                {
                    codeQuery = codeQuery.Where(c => c.Status == request.Status.Value);
                }

                var query = from c in codeQuery
                            join s in sales on c.SalesOrderItemId equals s.SalesOrderItemId
                            join p in planned on c.PlannedOrderId equals p.PlannedOrderId into pjoin
                            from p in pjoin.DefaultIfEmpty()
                            select new GetCodeReportListDto
                            {
                                CodeValue = c.CodeValue,
                                Status = c.Status,
                                SalesOrderNo = s.SalesOrderNo,
                                SalesItemNo = s.SalesItemNo,
                                PlannedOrderNo = p != null ? p.PlannedOrderNo : string.Empty,
                                ProducedAt = c.ProducedAt,
                                RecoverAt = c.RecoverAt,
                                UpdatedAt = c.UpdatedAt
                            };

                var items = await query
                    .OrderByDescending(x => x.UpdatedAt)
                    .ToListAsync(cancellationToken);

                var content = BuildExportContent(items, request.OnlyCodes);
                var bytes = new UTF8Encoding(true).GetBytes(content);

                return new ExportCodeReportDto
                {
                    Content = bytes
                };
            }

            private static string BuildExportContent(List<GetCodeReportListDto> items, bool onlyCodes)
            {
                var sb = new StringBuilder();

                if (onlyCodes)
                {
                    //sb.AppendLine("Code");
                    foreach (var item in items)
                    {
                        sb.AppendLine(NormalizeExportValue(item.CodeValue));
                    }

                    return sb.ToString();
                }

                sb.AppendLine("Code\tSalesOrderNo\tSalesItemNo\tPlannedOrderNo\tStatus\tProduced At");

                foreach (var item in items)
                {
                    //var statusText = GetStatusLabel(item.Status);
                    sb.Append(NormalizeExportValue(item.CodeValue));
                    sb.Append('\t');
                    sb.Append(NormalizeExportValue(item.SalesOrderNo));
                    sb.Append('\t');
                    sb.Append(NormalizeExportValue(item.SalesItemNo));
                    sb.Append('\t');
                    sb.Append(NormalizeExportValue(item.PlannedOrderNo));
                    sb.Append('\t');
                    sb.Append(NormalizeExportValue(item.Status.ToString()));
                    sb.Append('\t');
                    sb.AppendLine(item.ProducedAt?.ToString("dd.MM.yyyy HH:mm:ss"));
                }

                return sb.ToString();
            }

            private static string NormalizeExportValue(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return string.Empty;
                }

                return value.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
            }

            private static string GetStatusLabel(CodeStatus status)
            {
                return status switch
                {
                    CodeStatus.Available => "Hazır",
                    CodeStatus.Allocated => "Tasnif Edilmiş",
                    CodeStatus.ProducedOk => "Üretilmiş",
                    CodeStatus.Reject => "Iskarta",
                    CodeStatus.Scrap => "Fire",
                    CodeStatus.Void => "Boş",
                    _ => status.ToString()
                };
            }
        }
    }
}
