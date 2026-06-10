using AutoMapper;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetListCodesByPlannedOrderId;
using CryptoCodeControlAutomation.Domain.Entities;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Code, GetListCodesByPlannedOrderIdQueryDto>();
        }
    }
}
