using AutoMapper;
using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Application.Features.PlannedOrders.Rules;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetListCodesByPlannedOrderId
{
    public class GetListCodesByPlannedOrderIdQuery : IRequest<List<GetListCodesByPlannedOrderIdQueryDto>>
    {
        public string PlannedOrderNo { get; set; }

        public class GetListCodesByPlannedOrderIdQueryHandler : IRequestHandler<GetListCodesByPlannedOrderIdQuery, List<GetListCodesByPlannedOrderIdQueryDto>>
        {
            private readonly ICodeRepository _codeRepository;
            private readonly IPlannedOrderRepository _plannedOrderRepository;
            private readonly IMapper _mapper;
            private readonly PlannedOrderBusinessRules _plannedOrderBusinessRules;

            public GetListCodesByPlannedOrderIdQueryHandler(ICodeRepository codeRepository, IMapper mapper, IPlannedOrderRepository plannedOrderRepository, PlannedOrderBusinessRules plannedOrderBusinessRules)
            {
                _codeRepository = codeRepository;
                _mapper = mapper;
                _plannedOrderRepository = plannedOrderRepository;
                _plannedOrderBusinessRules = plannedOrderBusinessRules;
            }

            public async Task<List<GetListCodesByPlannedOrderIdQueryDto>> Handle(GetListCodesByPlannedOrderIdQuery request, CancellationToken cancellationToken)
            {
                var plannedOrder = await _plannedOrderRepository.Get(predicate: p => p.PlannedOrderNo == request.PlannedOrderNo, cancellationToken: cancellationToken);
                
                await _plannedOrderBusinessRules.PlannedOrderWasNotFound(plannedOrder);
                
                var codes = await _codeRepository.GetList(predicate: c => c.PlannedOrderId == plannedOrder.PlannedOrderId, cancellationToken: cancellationToken);

                return _mapper.Map<List<GetListCodesByPlannedOrderIdQueryDto>>(codes);
            }
        }
    }
}
