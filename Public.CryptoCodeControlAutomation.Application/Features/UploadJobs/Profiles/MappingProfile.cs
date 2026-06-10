using AutoMapper;
using CryptoCodeControlAutomation.Application.Features.UploadJobs.Queries.GetBySalesOrderItemId;
using CryptoCodeControlAutomation.Domain.Entities;

namespace CryptoCodeControlAutomation.Application.Features.UploadJobs.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UploadJob, GetUploadJobsBySalesOrderItemIdQueryDto>();
        }
    }
}
