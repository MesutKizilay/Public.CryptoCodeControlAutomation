using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.UpdateApprovalStatus
{
    public class UpdateSalesOrderItemApprovalStatusCommand : IRequest
    {
        public long SalesOrderItemId { get; set; }
        public SalesOrderItemApprovalStatus ApprovalStatus { get; set; }

        public class UpdateSalesOrderItemApprovalStatusCommandHandler : IRequestHandler<UpdateSalesOrderItemApprovalStatusCommand>
        {
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public UpdateSalesOrderItemApprovalStatusCommandHandler(ISalesOrderItemRepository salesOrderItemRepository, IHttpContextAccessor httpContextAccessor)
            {
                _salesOrderItemRepository = salesOrderItemRepository;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task Handle(UpdateSalesOrderItemApprovalStatusCommand request, CancellationToken cancellationToken)
            {
                var salesOrderItem = await _salesOrderItemRepository.Get(b => b.SalesOrderItemId == request.SalesOrderItemId, cancellationToken: cancellationToken);

                if (salesOrderItem == null)
                {
                    throw new BusinessException("Satış siparişi bulunamadı.");
                }

                salesOrderItem.ApprovalStatus = request.ApprovalStatus;
                var user = _httpContextAccessor.HttpContext?.User;
                var username = user?.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                    ?? user?.FindFirst(ClaimTypes.Email)?.Value
                    ?? user?.FindFirst(ClaimTypes.Name)?.Value;
                var approvedAt = DateTime.Now;

                if (request.ApprovalStatus == SalesOrderItemApprovalStatus.ProductionApproved)
                {
                    salesOrderItem.ProductionApprovedByUsername = username;
                    salesOrderItem.ProductionApprovedAt = approvedAt;
                }
                else if (request.ApprovalStatus == SalesOrderItemApprovalStatus.ShipmentApproved)
                {
                    salesOrderItem.ShipmentApprovedByUsername = username;
                    salesOrderItem.ShipmentApprovedAt = approvedAt;
                }

                salesOrderItem.UpdatedAt = DateTime.Now;

                await _salesOrderItemRepository.Update(salesOrderItem, cancellationToken);
            }
        }
    }
}
