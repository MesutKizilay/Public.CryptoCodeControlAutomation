using Core.Security.JWT;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Security
{
    public static class SecurityServicesRegistration
    {
        public static IServiceCollection AddSecurityervices(this IServiceCollection services)
        {
            services.AddScoped<ITokenHelper, JwtHelper>();

            return services;
        }
    }
}