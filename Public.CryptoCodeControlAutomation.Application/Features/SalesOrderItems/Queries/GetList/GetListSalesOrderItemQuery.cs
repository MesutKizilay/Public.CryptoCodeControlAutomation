using AutoMapper;
using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.GetList;
using Core.Persistence.Paging;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.GetList
{
    public class GetListSalesOrderItemQuery : IRequest<List<GetListSalesOrderItemDto>>
    {
        public class GetListSalesOrderItemQueryHandler : IRequestHandler<GetListSalesOrderItemQuery, List<GetListSalesOrderItemDto>>
        {
            private readonly ISalesOrderItemRepository _repository;
            private readonly IMapper _mapper;

            public GetListSalesOrderItemQueryHandler(ISalesOrderItemRepository repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<List<GetListSalesOrderItemDto>> Handle(GetListSalesOrderItemQuery request, CancellationToken cancellationToken)
            {
                var list = await _repository.GetList(cancellationToken: cancellationToken);
                return _mapper.Map<List<GetListSalesOrderItemDto>>(list);
            }
        }
    }
}