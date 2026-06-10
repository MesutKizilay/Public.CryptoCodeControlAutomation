using AutoMapper;
using Core.Persistence.Paging;
using Core.Security.Entities;

using CryptoCodeControlAutomation.Application.Features.Users.Commands.Create;
using CryptoCodeControlAutomation.Application.Features.Users.Commands.Update;
using CryptoCodeControlAutomation.Application.Features.Users.Queries.GetList;

namespace CryptoCodeControlAutomation.Application.Features.Users.Profiles
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Paginate<GetListUserDto>, Paginate<User>>().ReverseMap();
            CreateMap<User, GetListUserDto>()//.ForMember(destinationMember: u => u.OperationClaim, memberOptions: opt => opt.MapFrom(g => g.UserOperationClaims.Select(uop => uop.OperationClaim).FirstOrDefault()))
                                             .ReverseMap();

            //    CreateMap<User, GetListUserDto>().ForMember(d => d.OperationClaim,
            //opt => opt.MapFrom(u =>
            //    u.UserOperationClaims
            //     .Select(x => x.OperationClaim)
            //     .FirstOrDefault()));

            CreateMap<UpdateUserCommand, User>().ReverseMap();

            CreateMap<CreateUserCommand, User>().ReverseMap();
        }
    }
}