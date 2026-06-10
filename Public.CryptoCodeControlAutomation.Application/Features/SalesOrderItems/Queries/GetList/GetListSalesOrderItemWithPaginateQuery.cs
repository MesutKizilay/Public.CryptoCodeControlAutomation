using AutoMapper;
using Core.Application.Request;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using MediatR;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.GetList
{
    public class GetListSalesOrderItemWithPaginateQuery : IRequest<Paginate<GetListSalesOrderItemDto>>
    {
        public PageRequest PageRequest { get; set; }
        public DynamicQuery? DynamicQuery { get; set; }
        public bool WithDeleted { get; set; }

        public class GetListSalesOrderItemWithPaginateQueryHandler : IRequestHandler<GetListSalesOrderItemWithPaginateQuery, Paginate<GetListSalesOrderItemDto>>
        {
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly IMapper _mapper;

            public GetListSalesOrderItemWithPaginateQueryHandler(ISalesOrderItemRepository salesOrderItemRepository, IMapper mapper)
            {
                _salesOrderItemRepository = salesOrderItemRepository;
                _mapper = mapper;
            }

            public async Task<Paginate<GetListSalesOrderItemDto>> Handle(GetListSalesOrderItemWithPaginateQuery request, CancellationToken cancellationToken)
            {
                var sales = await _salesOrderItemRepository.GetListByDynamic(index: request.PageRequest.Index,
                                                                             size: request.PageRequest.Size,
                                                                             cancellationToken: cancellationToken,
                                                                             dynamic: request.DynamicQuery,
                                                                             withDeleted: request.WithDeleted);

                var dtos = _mapper.Map<Paginate<GetListSalesOrderItemDto>>(sales);
                return dtos;
            }
        }
    }
}