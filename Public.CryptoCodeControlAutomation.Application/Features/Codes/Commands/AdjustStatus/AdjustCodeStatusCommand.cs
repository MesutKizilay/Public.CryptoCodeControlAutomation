using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustStatus
{
    public class AdjustCodeStatusCommand : IRequest<AdjustCodeStatusResponse>,ISecuredRequest
    {
        public long? SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }
        public CodeStatus FromStatus { get; set; }
        public CodeStatus ToStatus { get; set; }
        public int Quantity { get; set; }
        public DateTime? ShiftDate { get; set; }
        public string Reason { get; set; } = string.Empty;

        public string[] Roles => new[] { "Supervisor" };

        public class AdjustCodeStatusCommandHandler : IRequestHandler<AdjustCodeStatusCommand, AdjustCodeStatusResponse>
        {
            private readonly ICodeRepository _codeRepository;
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly ICodeAdjustmentLogRepository _codeAdjustmentLogRepository;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public AdjustCodeStatusCommandHandler(ICodeRepository codeRepository, ISalesOrderItemRepository salesOrderItemRepository, ICodeAdjustmentLogRepository codeAdjustmentLogRepository, IHttpContextAccessor httpContextAccessor)
            {
                _codeRepository = codeRepository;
                _salesOrderItemRepository = salesOrderItemRepository;
                _codeAdjustmentLogRepository = codeAdjustmentLogRepository;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task<AdjustCodeStatusResponse> Handle(AdjustCodeStatusCommand request, CancellationToken cancellationToken)
            {
                var query = ApplySelection(_codeRepository.Query(), request)
                    .Where(c => c.Status == request.FromStatus);

                if (request.FromStatus == CodeStatus.ProducedOk && request.ToStatus == CodeStatus.Allocated)
                {
                    var sourceShiftDate = request.ShiftDate!.Value.Date;
                    query = query.Where(c => c.ShiftDate == sourceShiftDate);
                }

                var codes = await query
                    .OrderBy(c => c.AllocatedAt == null)
                    .ThenBy(c => c.AllocatedAt)
                    .ThenBy(c => c.CodeId)
                    .Take(request.Quantity)
                    .ToListAsync(cancellationToken);

                if (codes.Count < request.Quantity)
                {
                    throw new BusinessException($"Kaynak durumda yeterli kod yok. Bulunan: {codes.Count}, istenen: {request.Quantity}.");
                }

                var now = DateTime.Now;
                var log = new CodeAdjustmentLog
                {
                    OperationType = "StatusChange",
                    SalesOrderItemId = request.SalesOrderItemId,
                    PlannedOrderId = request.PlannedOrderId,
                    FromStatus = request.FromStatus,
                    ToStatus = request.ToStatus,
                    FromShiftDate = request.FromStatus == CodeStatus.ProducedOk ? request.ShiftDate!.Value.Date : null,
                    ToShiftDate = request.ToStatus == CodeStatus.ProducedOk ? request.ShiftDate!.Value.Date : null,
                    Quantity = codes.Count,
                    Reason = request.Reason.Trim(),
                    CreatedBy = GetCurrentUsername(),
                    CreatedAt = now
                };

                if (request.FromStatus == CodeStatus.Allocated && request.ToStatus == CodeStatus.ProducedOk)
                {
                    await ApplyAllocatedToProduced(codes, request.ShiftDate!.Value.Date, now, log, cancellationToken);
                }
                else
                {
                    ApplyProducedToAllocated(codes, now, log);
                }

                await _codeAdjustmentLogRepository.Add(log, cancellationToken);

                return new AdjustCodeStatusResponse
                {
                    UpdatedCount = codes.Count,
                    CodeAdjustmentLogId = log.CodeAdjustmentLogId,
                    Message = $"{codes.Count} kod güncellendi."
                };
            }

            private async Task ApplyAllocatedToProduced(
                List<Code> codes,
                DateTime shiftDate,
                DateTime updatedAt,
                CodeAdjustmentLog log,
                CancellationToken cancellationToken)
            {
                if (codes.Any(c => !c.AllocatedAt.HasValue))
                {
                    throw new BusinessException("Tasnif tarihi olmayan kodlar üretildi durumuna alýnamaz.");
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

                    code.Status = CodeStatus.ProducedOk;
                    code.ProducedAt = code.AllocatedAt;
                    code.ShiftDate = shiftDate;
                    code.ExpirationDate = CodeAdjustmentDateHelper.CalculateExpirationDate(
                        shiftDate,
                        salesOrderItem.ShelfLifeValue,
                        (byte)salesOrderItem.ShelfLifeUnit);
                    code.UpdatedAt = updatedAt;

                    AddLogItem(log, code, oldStatus, oldShiftDate, oldProducedAt, oldExpirationDate);
                }
            }

            private static void ApplyProducedToAllocated(List<Code> codes, DateTime updatedAt, CodeAdjustmentLog log)
            {
                foreach (var code in codes)
                {
                    var oldStatus = code.Status;
                    var oldShiftDate = code.ShiftDate;
                    var oldProducedAt = code.ProducedAt;
                    var oldExpirationDate = code.ExpirationDate;

                    code.Status = CodeStatus.Allocated;
                    code.ProducedAt = null;
                    code.ShiftDate = null;
                    code.ExpirationDate = null;
                    code.UpdatedAt = updatedAt;

                    AddLogItem(log, code, oldStatus, oldShiftDate, oldProducedAt, oldExpirationDate);
                }
            }

            private static void AddLogItem(
                CodeAdjustmentLog log,
                Code code,
                CodeStatus oldStatus,
                DateTime? oldShiftDate,
                DateTime? oldProducedAt,
                DateTime? oldExpirationDate)
            {
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

            private static IQueryable<Code> ApplySelection(IQueryable<Code> query, AdjustCodeStatusCommand request)
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
