using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Extension;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Immutable;

namespace Core.Application.Pipelines.Authorization
{
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseRequest, ISecuredRequest
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationBehavior(IHttpContextAccessor httpContextAccessor)
        {            
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {            
            if (!_httpContextAccessor.HttpContext.User.Claims.Any())
                throw new AuthorizationException("You are not authenticated.");

            if (request.Roles.Any())
            {
                ICollection<string>? userRoleClaims = _httpContextAccessor.HttpContext.User.GetRoleClaims() ?? [];
                bool isNotMatchedAUserRoleClaimWithRequestRoles = userRoleClaims.FirstOrDefault(userRoleClaim => request.Roles.Contains(userRoleClaim)) == null;
                
                if (isNotMatchedAUserRoleClaimWithRequestRoles)
                    throw new AuthorizationException("Bu işlemi yapmak için yetkili değilsiniz.");
            }

            TResponse response = await next();
            return response;
        }
    }
}