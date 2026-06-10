using AutoMapper;
using Core.Security.Entities;
using CryptoCodeControlAutomation.Application.Features.Roles.Queries.GetList;

namespace CryptoCodeControlAutomation.Application.Features.Roles.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Role,GetListRoleDto>().ReverseMap();
        }
    }
}