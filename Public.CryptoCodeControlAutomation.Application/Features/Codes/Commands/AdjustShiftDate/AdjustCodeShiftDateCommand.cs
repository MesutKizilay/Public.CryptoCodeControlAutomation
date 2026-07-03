using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustStatus;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustShiftDate
{
    public class AdjustCodeShiftDateCommand : IRequest<AdjustCodeShiftDateResponse>, ISecuredRequest
    {
        public long? SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }
        public DateTime? FromShiftDate { get; set; }
        public DateTime? ToShiftDate { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string[] Roles => new[] { "Supervisor" };


        public class AdjustCodeShiftDateCommandHandler : IRequestHandler<AdjustCodeShiftDateCommand, AdjustCodeShiftDateResponse>
        {
            private readonly ICodeRepository _codeRepository;
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly ICodeAdjustmentLogRepository _codeAdjustmentLogRepository;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public AdjustCodeShiftDateCommandHandler(
                ICodeRepository codeRepository,
                ISalesOrderItemRepository salesOrderItemRepository,
                ICodeAdjustmentLogRepository codeAdjustmentLogRepository,
                IHttpContextAccessor httpContextAccessor)
            {
                _codeRepository = codeRepository;
                _salesOrderItemRepository = salesOrderItemRepository;
                _codeAdjustmentLogRepository = codeAdjustmentLogRepository;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task<AdjustCodeShiftDateResponse> Handle(AdjustCodeShiftDateCommand request, CancellationToken cancellationToken)
            {
                var fromShiftDate = request.FromShiftDate!.Value.Date;
                var toShiftDate = request.ToShiftDate!.Value.Date;

                var query = ApplySelection(_codeRepository.Query(), request)
                    .Where(c => c.Status == CodeStatus.ProducedOk &&
                                c.ShiftDate.HasValue &&
                                c.ShiftDate.Value.Date == fromShiftDate);

                var codes = await query
                    .OrderBy(c => c.AllocatedAt == null)
                    .ThenBy(c => c.AllocatedAt)
                    .ThenBy(c => c.CodeId)
                    .Take(request.Quantity)
                    .ToListAsync(cancellationToken);

                if (codes.Count < request.Quantity)
                {
                    throw new BusinessException($"Seçilen üretim tarihinde yeterli miktarda üretilmiþ kod bulunamadý. Bulunan: {codes.Count}, istenen: {request.Quantity}.");
                }

                var salesOrderItemIds = codes.Select(c => c.SalesOrderItemId).Distinct().ToList();
                var salesOrderItems = await _salesOrderItemRepository.Query()
                    .IgnoreQueryFilters()
                    .Where(s => salesOrderItemIds.Contains(s.SalesOrderItemId))
                    .Select(s => new
                    {
                        s.SalesOrderItemId,
                        s.ShelfLifeValue,
                        s.ShelfLifeUnit
                    })
                    .ToDictionaryAsync(s => s.SalesOrderItemId, cancellationToken);

                var now = DateTime.Now;
                var log = new CodeAdjustmentLog
                {
                    OperationType = "ShiftDateChange",
                    SalesOrderItemId = request.SalesOrderItemId,
                    PlannedOrderId = request.PlannedOrderId,
                    FromStatus = CodeStatus.ProducedOk,
                    ToStatus = CodeStatus.ProducedOk,
                    FromShiftDate = fromShiftDate,
                    ToShiftDate = toShiftDate,
                    Quantity = codes.Count,
                    Reason = request.Reason.Trim(),
                    CreatedBy = GetCurrentUsername(),
                    CreatedAt = now
                };

                foreach (var code in codes)
                {
                    if (!salesOrderItems.TryGetValue(code.SalesOrderItemId, out var salesOrderItem))
                    {
                        throw new BusinessException("Satýþ sipariþi bulunamadý.");
                    }

                    var oldStatus = code.Status;
                    var oldShiftDate = code.ShiftDate;
                    var oldProducedAt = code.ProducedAt;
                    var oldExpirationDate = code.ExpirationDate;

                    code.ShiftDate = toShiftDate;
                    code.ExpirationDate = CodeAdjustmentDateHelper.CalculateExpirationDate(
                        toShiftDate,
                        salesOrderItem.ShelfLifeValue,
                        (byte)salesOrderItem.ShelfLifeUnit);
                    code.UpdatedAt = now;

                    log.Items.Add(new CodeAdjustmentLogItem
                    {
                        CodeId = code.CodeId,
                        CodeValue = code.CodeValue,
                        OldStatus = oldStatus,
                        NewStatus = code.Status,
                        OldShiftDate = oldShiftDate,
                        NewShiftDate = code.ShiftDate,
                        OldProducedAt = oldProducedAt,
                        NewProducedAt = code.ProducedAt,
                        OldExpirationDate = oldExpirationDate,
                        NewExpirationDate = code.ExpirationDate
                    });
                }

                await _codeAdjustmentLogRepository.Add(log, cancellationToken);

                return new AdjustCodeShiftDateResponse
                {
                    UpdatedCount = codes.Count,
                    CodeAdjustmentLogId = log.CodeAdjustmentLogId,
                    Message = $"{codes.Count} kodun üretim tarihi güncellendi."
                };
            }

            private static IQueryable<Code> ApplySelection(IQueryable<Code> query, AdjustCodeShiftDateCommand request)
            {
                var hasSalesOrderItem = request.SalesOrderItemId.HasValue && request.SalesOrderItemId.Value > 0;
                var hasPlannedOrder = request.PlannedOrderId.HasValue && request.PlannedOrderId.Value > 0;

                if (hasSalesOrderItem)
                {
                    query = query.Where(c => c.SalesOrderItemId == request.SalesOrderItemId!.Value);
                }

                if (hasPlannedOrder)
                {
                    query = query.Where(c => c.PlannedOrderId == request.PlannedOrderId!.Value);
                }

                return query;
            }

            private string? GetCurrentUsername()
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                    ?? user?.FindFirst(ClaimTypes.Email)?.Value
                    ?? user?.FindFirst(ClaimTypes.Name)?.Value;
            }
        }
    }
}
