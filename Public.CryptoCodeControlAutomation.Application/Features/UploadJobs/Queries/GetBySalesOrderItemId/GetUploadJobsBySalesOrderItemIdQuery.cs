using AutoMapper;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using MediatR;

namespace CryptoCodeControlAutomation.Application.Features.UploadJobs.Queries.GetBySalesOrderItemId
{
    public class GetUploadJobsBySalesOrderItemIdQuery : IRequest<List<GetUploadJobsBySalesOrderItemIdQueryDto>>
    {
        public long SalesOrderItemId { get; set; }

        public class GetUploadJobsBySalesOrderItemIdQueryHandler : IRequestHandler<GetUploadJobsBySalesOrderItemIdQuery, List<GetUploadJobsBySalesOrderItemIdQueryDto>>
        {
            private readonly IUploadJobRepository _uploadJobRepository;
            private readonly IMapper _mapper;

            public GetUploadJobsBySalesOrderItemIdQueryHandler(IUploadJobRepository uploadJobRepository, IMapper mapper)
            {
                _uploadJobRepository = uploadJobRepository;
                _mapper = mapper;
            }

            public async Task<List<GetUploadJobsBySalesOrderItemIdQueryDto>> Handle(GetUploadJobsBySalesOrderItemIdQuery request, CancellationToken cancellationToken)
            {
                var jobs = await _uploadJobRepository.GetList(predicate: j => j.SalesOrderItemId == request.SalesOrderItemId, cancellationToken: cancellationToken);

                //jobs.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));

                var uploadJobsDto = _mapper.Map<List<GetUploadJobsBySalesOrderItemIdQueryDto>>(jobs);

                return uploadJobsDto;
            }
        }
    }
}
