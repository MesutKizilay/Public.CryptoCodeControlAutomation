using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using CryptoCodeControlAutomation.Persistence.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Public.CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustStatus;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustStatus
{
    public class AdjustCodeStatusCommand : IRequest<AdjustCodeStatusResponse>,ISecuredRequest
    {
        private const string SensitiveTransitionPassword = "kgt";

        public long? SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }
        public CodeStatus FromStatus { get; set; }
        public CodeStatus ToStatus { get; set; }
        public int Quantity { get; set; }
        public DateTime? ShiftDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Password { get; set; }

        public string[] Roles => new[] { "Supervisor" };

        public class AdjustCodeStatusCommandHandler : IRequestHandler<AdjustCodeStatusCommand, AdjustCodeStatusResponse>
        {
            private readonly ICodeRepository _codeRepository;
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly IPlannedOrderSalesLinkRepository _plannedOrderSalesLinkRepository;
            private readonly ICodeAdjustmentLogRepository _codeAdjustmentLogRepository;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public AdjustCodeStatusCommandHandler(
                ICodeRepository codeRepository,
                ISalesOrderItemRepository salesOrderItemRepository,
                IPlannedOrderSalesLinkRepository plannedOrderSalesLinkRepository,
                ICodeAdjustmentLogRepository codeAdjustmentLogRepository,
                IHttpContextAccessor httpContextAccessor)
            {
                _codeRepository = codeRepository;
                _salesOrderItemRepository = salesOrderItemRepository;
                _plannedOrderSalesLinkRepository = plannedOrderSalesLinkRepository;
                _codeAdjustmentLogRepository = codeAdjustmentLogRepository;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task<AdjustCodeStatusResponse> Handle(AdjustCodeStatusCommand request, CancellationToken cancellationToken)
            {
                if (IsAvailableTransition(request))
                {
                    ValidateSensitiveTransitionPassword(request.Password);
                    await ResolvePlannedOrderSelection(request, cancellationToken);

                    return request.FromStatus == CodeStatus.Available
                        ? await ApplyAvailableToAllocated(request, cancellationToken)
                        : await ApplyAllocatedToAvailable(request, cancellationToken);
                }

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

            private async Task<AdjustCodeStatusResponse> ApplyAvailableToAllocated(
                AdjustCodeStatusCommand request,
                CancellationToken cancellationToken)
            {
                List<CodeAllocationResult> allocatedCodes;

                try
                {
                    allocatedCodes = await _codeRepository.AllocateAvailableCodes(
                        request.PlannedOrderId!.Value,
                        request.Quantity,
                        cancellationToken);
                }
                catch (SqlException exception) when (exception.Number is >= 62000 and < 63000)
                {
                    var message = exception.Errors
                        .Cast<SqlError>()
                        .FirstOrDefault(error => error.Number == exception.Number)
                        ?.Message ?? exception.Message;

                    throw new BusinessException(message);
                }
                catch (InvalidOperationException exception)
                {
                    throw new BusinessException(exception.Message);
                }

                if (allocatedCodes.Count == 0)
                    throw new BusinessException("Tahsis edilen kod bulunamadı.");

                var now = DateTime.Now;
                var log = CreateStatusLog(request, allocatedCodes.Count, now);
                log.CreatedBy = GetCurrentUsername();

                foreach (var code in allocatedCodes)
                {
                    log.Items.Add(new CodeAdjustmentLogItem
                    {
                        CodeId = code.CodeId,
                        CodeValue = code.CodeValue,
                        OldStatus = CodeStatus.Available,
                        NewStatus = CodeStatus.Allocated
                    });
                }

                await _codeAdjustmentLogRepository.Add(log, cancellationToken);

                return CreateResponse(allocatedCodes.Count, log.CodeAdjustmentLogId);
            }

            private async Task<AdjustCodeStatusResponse> ApplyAllocatedToAvailable(
                AdjustCodeStatusCommand request,
                CancellationToken cancellationToken)
            {
                var codes = await ApplySelection(_codeRepository.Query(), request)
                    .AsNoTracking()
                    .Where(c => c.Status == CodeStatus.Allocated)
                    .OrderBy(c => c.AllocatedAt == null)
                    .ThenBy(c => c.AllocatedAt)
                    .ThenBy(c => c.CodeId)
                    .Take(request.Quantity)
                    .ToListAsync(cancellationToken);

                if (codes.Count < request.Quantity)
                {
                    throw new BusinessException($"Kaynak durumda yeterli kod yok. Bulunan: {codes.Count}, istenen: {request.Quantity}.");
                }

                var updatedCount = await _codeRepository.ResetAllocatedCodesToAvailable(
                    codes.Select(c => c.CodeId).ToList(),
                    request.PlannedOrderId!.Value,
                    cancellationToken);

                if (updatedCount != codes.Count)
                    throw new BusinessException("Seçilen kodların tamamı kullanılabilir duruma alınamadı.");

                var now = DateTime.Now;
                var log = CreateStatusLog(request, updatedCount, now);
                log.CreatedBy = GetCurrentUsername();

                foreach (var code in codes)
                {
                    var oldStatus = code.Status;
                    var oldShiftDate = code.ShiftDate;
                    var oldProducedAt = code.ProducedAt;
                    var oldExpirationDate = code.ExpirationDate;

                    code.Status = CodeStatus.Available;
                    code.ShiftDate = null;
                    code.ProducedAt = null;
                    code.ExpirationDate = null;

                    AddLogItem(log, code, oldStatus, oldShiftDate, oldProducedAt, oldExpirationDate);
                }

                await _codeAdjustmentLogRepository.Add(log, cancellationToken);

                return CreateResponse(updatedCount, log.CodeAdjustmentLogId);
            }

            private async Task ResolvePlannedOrderSelection(AdjustCodeStatusCommand request, CancellationToken cancellationToken)
            {
                var hasSalesOrderItem = request.SalesOrderItemId.HasValue && request.SalesOrderItemId.Value > 0;
                var hasPlannedOrder = request.PlannedOrderId.HasValue && request.PlannedOrderId.Value > 0;

                if (hasSalesOrderItem && hasPlannedOrder)
                    return;

                var links = _plannedOrderSalesLinkRepository.Query();
                PlannedOrderSalesLink? link;

                if (hasSalesOrderItem)
                {
                    link = await links.FirstOrDefaultAsync(
                        item => item.SalesOrderItemId == request.SalesOrderItemId!.Value,
                        cancellationToken);

                    if (link is null)
                        throw new BusinessException("Bu satış siparişine bağlı planlı sipariş bulunamadı.");
                }
                else
                {
                    link = await links.FirstOrDefaultAsync(
                        item => item.PlannedOrderId == request.PlannedOrderId!.Value,
                        cancellationToken);

                    if (link is null)
                        throw new BusinessException("Bu planlı siparişe bağlı satış siparişi bulunamadı.");
                }

                request.SalesOrderItemId = link.SalesOrderItemId;
                request.PlannedOrderId = link.PlannedOrderId;
            }

            private static CodeAdjustmentLog CreateStatusLog(AdjustCodeStatusCommand request, int quantity, DateTime createdAt)
            {
                return new CodeAdjustmentLog
                {
                    OperationType = "StatusChange",
                    SalesOrderItemId = request.SalesOrderItemId,
                    PlannedOrderId = request.PlannedOrderId,
                    FromStatus = request.FromStatus,
                    ToStatus = request.ToStatus,
                    Quantity = quantity,
                    Reason = request.Reason.Trim(),
                    CreatedAt = createdAt
                };
            }

            private AdjustCodeStatusResponse CreateResponse(int updatedCount, long logId)
            {
                return new AdjustCodeStatusResponse
                {
                    UpdatedCount = updatedCount,
                    CodeAdjustmentLogId = logId,
                    Message = $"{updatedCount} kod güncellendi."
                };
            }

            private static bool IsAvailableTransition(AdjustCodeStatusCommand request)
            {
                return request.FromStatus == CodeStatus.Available && request.ToStatus == CodeStatus.Allocated
                    || request.FromStatus == CodeStatus.Allocated && request.ToStatus == CodeStatus.Available;
            }

            private static void ValidateSensitiveTransitionPassword(string? password)
            {
                if (password != SensitiveTransitionPassword)
                    throw new BusinessException("Hatalı şifre girdiniz.");
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
                    throw new BusinessException("Tasnif tarihi olmayan kodlar üretildi durumuna alınamaz.");
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
                        throw new BusinessException("Satış siparişi bulunamadı.");
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
