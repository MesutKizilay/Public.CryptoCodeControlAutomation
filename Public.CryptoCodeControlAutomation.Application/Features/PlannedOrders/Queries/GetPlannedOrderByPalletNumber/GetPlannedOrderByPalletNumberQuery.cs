using CryptoCodeControlAutomation.Application.Services.Validations;
using MediatR;

namespace CryptoCodeControlAutomation.Application.Features.PlannedOrders.Queries.GetPlannedOrderByPalletNumber
{
    public class GetPlannedOrderByPalletNumberQuery : IRequest<GetPlannedOrderByPalletNumberDto>
    {
        public string TbNo { get; set; }

        public class GetPlannedOrderByPalletNumberQueryHandler : IRequestHandler<GetPlannedOrderByPalletNumberQuery, GetPlannedOrderByPalletNumberDto>
        {
            private readonly IPlannedOrderService _plannedOrderService;

            public GetPlannedOrderByPalletNumberQueryHandler(IPlannedOrderService plannedOrderService)
            {
                _plannedOrderService = plannedOrderService;
            }

            public async Task<GetPlannedOrderByPalletNumberDto> Handle(GetPlannedOrderByPalletNumberQuery request, CancellationToken cancellationToken)
            {
                return await _plannedOrderService.GetPlannedOrderByPalletNumber(request.TbNo, cancellationToken);
            }
        }
    }
}
