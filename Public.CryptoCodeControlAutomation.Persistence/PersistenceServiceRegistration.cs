using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Persistence.Contexts;
using CryptoCodeControlAutomation.Persistence.Repositories;

namespace CryptoCodeControlAutomation.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CryptoContext>(options => options.UseSqlServer(configuration.GetConnectionString("MsSqlConnectionString"),
                                                                                 sqlServerOptionsAction =>
                                                                                 {
                                                                                     sqlServerOptionsAction.CommandTimeout(300);
                                                                                     sqlServerOptionsAction.EnableRetryOnFailure();
                                                                                 })
                                                                   .ConfigureWarnings(c => c.Ignore(RelationalEventId.PendingModelChangesWarning)));

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserRoleRepository, UserRoleRepository>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<ISalesOrderItemRepository, SalesOrderItemRepository>();
            services.AddTransient<IUploadJobRepository, UploadJobRepository>();
            services.AddTransient<ICodeRepository, CodeRepository>();
            services.AddTransient<ICodeAdjustmentLogRepository, CodeAdjustmentLogRepository>();
            services.AddTransient<IPlannedOrderRepository, PlannedOrderRepository>();
            services.AddTransient<IPlannedOrderSalesLinkRepository, PlannedOrderSalesLinkRepository>();

            return services;
        }
    }
}
