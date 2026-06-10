using AutoMapper;
using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.GetList;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.GetById
{
    public class GetByIdSalesOrderItemQuery : IRequest<GetListSalesOrderItemDto>
    {
        public long Id { get; set; }

        public class GetByIdSalesOrderItemQueryHandler : IRequestHandler<GetByIdSalesOrderItemQuery, GetListSalesOrderItemDto>
        {
            private readonly ISalesOrderItemRepository _repository;
            private readonly IMapper _mapper;

            public GetByIdSalesOrderItemQueryHandler(ISalesOrderItemRepository repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<GetListSalesOrderItemDto> Handle(GetByIdSalesOrderItemQuery request, CancellationToken cancellationToken)
            {
                var entity = await _repository.Get(b => b.SalesOrderItemId == request.Id, cancellationToken: cancellationToken);
                return _mapper.Map<GetListSalesOrderItemDto>(entity);
            }
        }
    }
}