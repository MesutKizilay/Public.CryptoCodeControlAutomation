using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CryptoCodeControlAutomation.Application.Services.Validations;
using CryptoCodeControlAutomation.Application.Services.LdapService;
using CryptoCodeControlAutomation.Infrastructure.Services.LdapManagerService;
using CryptoCodeControlAutomation.Infrastructure.Services.PlannedOrderManagerService;
using CryptoCodeControlAutomation.Infrastructure.Services.SalesOrderItemManagerService;
using System.Net.Http.Headers;
using System.Text;

namespace CryptoCodeControlAutomation.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration["SalesOrderItemApi:BaseUrl"];
            var username = configuration["SalesOrderItemApi:Username"];
            var password = configuration["SalesOrderItemApi:Password"];
            services.AddHttpClient("SalesOrderItemApi", client =>
            {
                if (!string.IsNullOrWhiteSpace(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);

                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
                }
            });

            services.AddTransient<ISalesOrderItemService, SalesOrderItemManager>();
            services.AddTransient<IPlannedOrderService, PlannedOrderManager>();
            services.AddTransient<ILdapService, LdapManager>();
            //services.AddScoped<LdapSettings>();


            return services;
        }
    }
}
