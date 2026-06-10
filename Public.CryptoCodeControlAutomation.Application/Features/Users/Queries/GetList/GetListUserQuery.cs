using AutoMapper;
using Core.Application.Pipelines.Authorization;
using Core.Application.Request;
using Core.Persistence.Paging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CryptoCodeControlAutomation.Application.Services.Repositories;

namespace CryptoCodeControlAutomation.Application.Features.Users.Queries.GetList
{
    public class GetListUserQuery : IRequest<Paginate<GetListUserDto>>
    {
        public PageRequest PageRequest { get; set; }
        public bool WithDeleted { get; set; }

        //public string[] Roles => new string[] { "Admin" };

        public class GetListUserQueryHandler : IRequestHandler<GetListUserQuery, Paginate<GetListUserDto>>
        {
            private readonly IUserRepository _userRepository;
            private readonly IMapper _mapper;

            public GetListUserQueryHandler(IUserRepository userRepository, IMapper mapper)
            {
                _userRepository = userRepository;
                _mapper = mapper;
            }

            public async Task<Paginate<GetListUserDto>> Handle(GetListUserQuery request, CancellationToken cancellationToken)
            {
                var users = await _userRepository.GetListWithPaginate(index: request.PageRequest.Index,
                                                                      size: request.PageRequest.Size,
                                                                      cancellationToken: cancellationToken,
                                                                      include: u => u.Include(u => u.UserRoles).ThenInclude(ur => ur.Role),
                                                                      withDeleted: request.WithDeleted);

                var userDtos = _mapper.Map<Paginate<GetListUserDto>>(users);
                //await Task.Delay(5000);
                return userDtos;
            }
        }
    }
}