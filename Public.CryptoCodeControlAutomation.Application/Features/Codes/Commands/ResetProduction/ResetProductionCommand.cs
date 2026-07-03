using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Application.Pipelines.Authorization;
using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.ResetProduction
{
    public class ResetProductionCommand : IRequest<ResetProductionResponse>, ISecuredRequest
    {
        private const string ResetPassword = "kgt";

        public long? SalesOrderItemId { get; set; }
        public long? PlannedOrderId { get; set; }
        public string Password { get; set; } = string.Empty;

        public string[] Roles => new[] { "Supervisor" };

        public class ResetProductionCommandHandler : IRequestHandler<ResetProductionCommand, ResetProductionResponse>
        {
            private readonly ICodeRepository _codeRepository;
            private readonly ICodeAdjustmentLogRepository _codeAdjustmentLogRepository;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public ResetProductionCommandHandler(ICodeRepository codeRepository, ICodeAdjustmentLogRepository codeAdjustmentLogRepository, IHttpContextAccessor httpContextAccessor)
            {
                _codeRepository = codeRepository;
                _codeAdjustmentLogRepository = codeAdjustmentLogRepository;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task<ResetProductionResponse> Handle(ResetProductionCommand request, CancellationToken cancellationToken)
            {
                if (request.Password != ResetPassword)
                {
                    throw new BusinessException("Hatalý þifre girdiniz.");
                }

                var updatedCount = await _codeRepository.ResetProduction(request.SalesOrderItemId, request.PlannedOrderId, cancellationToken);

                var now = DateTime.Now;
                var log = new CodeAdjustmentLog
                {
                    OperationType = "ProductionReset",
                    SalesOrderItemId = request.SalesOrderItemId,
                    PlannedOrderId = request.PlannedOrderId,
                    FromStatus = null,
                    ToStatus = CodeStatus.Available,
                    FromShiftDate = null,
                    ToShiftDate = null,
                    Quantity = updatedCount,
                    Reason = "Production reset",
                    CreatedBy = GetCurrentUsername(),
                    CreatedAt = now
                };

                await _codeAdjustmentLogRepository.Add(log, cancellationToken);

                return new ResetProductionResponse
                {
                    UpdatedCount = updatedCount,
                    CodeAdjustmentLogId = log.CodeAdjustmentLogId,
                    Message = $"{updatedCount} kod sýfýrlandý."
                };
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
