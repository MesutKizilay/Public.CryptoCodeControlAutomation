using AutoMapper;
using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;

namespace CryptoCodeControlAutomation.Application.Features.Roles.Queries.GetList
{
    public class GetListRoleQuery : IRequest<List<GetListRoleDto>>
    {
        public class GetListRoleQueryHandler : IRequestHandler<GetListRoleQuery, List<GetListRoleDto>>
        {
            private readonly IRoleRepository _roleRepository;
            private readonly IMapper _mapper;

            public GetListRoleQueryHandler(IMapper mapper, IRoleRepository roleRepository)
            {
                _mapper = mapper;
                _roleRepository = roleRepository;
            }

            public async Task<List<GetListRoleDto>> Handle(GetListRoleQuery request, CancellationToken cancellationToken)
            {
                var userRoles = await _roleRepository.GetList();
                var roleDto = _mapper.Map<List<GetListRoleDto>>(userRoles);

                return roleDto;
            }
        }
    }
}