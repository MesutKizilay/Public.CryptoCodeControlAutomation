using Core.CrossCuttingConcerns.Exceptions.Extensions;
using Core.Security;
using Core.Security.Encryption;
using Core.Security.JWT;
using CryptoCodeControlAutomation.Application;
using CryptoCodeControlAutomation.Infrastructure;
using CryptoCodeControlAutomation.Persistence;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using CryptoCodeControlAutomation.Presentation.Services;
using static CryptoCodeControlAutomation.Presentation.Controllers.LdapController;

namespace CryptoCodeControlAutomation.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<LdapAuthenticationService>();
            builder.Services.AddScoped<LdapSettings>();
            builder.Services.AddSingleton<MoxaTcpDemoService>();

            // Configure Kestrel for large file uploads (100 MB limit)
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 1000 * 1024 * 1024; // 100 MB
            });

            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 1000 * 1024 * 1024; // 100 MB
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = 1000 * 1024 * 1024;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure FormOptions for large file uploads

            builder.Services.AddPersistenceServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddSecurityervices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddHangfire(config =>
            {
                var connectionString = builder.Configuration.GetConnectionString("MsSqlConnectionString");
                config.UseSimpleAssemblyNameTypeSerializer()
                      .UseRecommendedSerializerSettings()
                      .UseSqlServerStorage(connectionString);
            });

            builder.Services.AddHangfireServer(options =>
            {
                options.WorkerCount = 1;
            });

            //builder.Services.AddMvc(config =>
            //{
            //    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            //    config.Filters.Add(new AuthorizeFilter(policy));
            //});

            TokenOptions? tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>();
            builder.Services
                            //.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddAuthentication(options =>
                            {
                                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                            })
                            //.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                            .AddJwtBearer(options =>
                            {
                                options.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ValidateIssuer = true,
                                    ValidateAudience = true,
                                    ValidateLifetime = true,
                                    ValidIssuer = tokenOptions.Issuer,
                                    ValidAudience = tokenOptions.Audience,
                                    ValidateIssuerSigningKey = true,
                                    IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey),
                                    ClockSkew = TimeSpan.Zero
                                };

                                options.Events = new JwtBearerEvents
                                {
                                    OnMessageReceived = ctx =>
                                    {
                                        if (ctx.Request.Cookies.TryGetValue("AccessToken", out var token))
                                            ctx.Token = token;
                                        //else
                                        //    ctx.Response.Redirect($"/Auth/Login");

                                        return Task.CompletedTask;
                                    }
                                };
                            })
                            .AddCookie(config =>
                            {
                                //config.Cookie.HttpOnly = true;
                                //config.ExpireTimeSpan = TimeSpan.FromSeconds(10);
                                //config.SlidingExpiration = true;
                                
                                config.LoginPath = "/Auth/Login";
                                config.AccessDeniedPath = "/Home/AccessDenied";
                            });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                                         .RequireRole("Admin", "Supervisor")
                                                                         .Build();

                options.AddPolicy("OperatorOnly", policy =>
                    policy.RequireRole("Operator"));

                options.AddPolicy("AdminSupervisorOrOperator", policy =>
                    policy.RequireRole("Admin", "Supervisor", "Operator"));

                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
            });

            // Named HttpClient to call external SAP API (base URL configured via SapApi:BaseUrl in appsettings)
            //builder.Services.AddHttpClient("SapApi", client =>
            //{
            //    var baseUrl = builder.Configuration["SapApi:BaseUrl"] ?? "https://localhost:7281/";
            //    client.BaseAddress = new Uri(baseUrl);
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //    client.Timeout = TimeSpan.FromDays(1);


            //    // If you want to pass an API key from config: SapApi:ApiKey
            //    var apiKey = builder.Configuration["SapApi:ApiKey"];
            //    if (!string.IsNullOrWhiteSpace(apiKey))
            //    {
            //        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            //    }
            //});

            //builder.Services.AddRateLimiter(options =>
            //{
            //    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            //    options.AddPolicy("fixed-by-user", httpContext =>
            //        RateLimitPartition.GetFixedWindowLimiter(
            //            partitionKey: httpContext.User.Identity?.Name?.ToString(),
            //            factory: _ => new FixedWindowRateLimiterOptions
            //            {
            //                PermitLimit = 10,
            //                Window = TimeSpan.FromMinutes(1)
            //            }));
            //});

            var app = builder.Build();

            //if (app.Environment.IsProduction())
            app.ConfigureCustomExceptionMiddleware();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/NotFound404");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            // Optional: enable dashboard if you want it exposed
            //app.UseHangfireDashboard("/hangfire");
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthFilter() }
            });

            //app.UseRateLimiter();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=SalesOrderItems}/{action=SalesOrderItems}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }

    public sealed class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var http = context.GetHttpContext();
            if (http.User?.Identity?.IsAuthenticated != true)
            {
                //http.Response.Redirect("/Auth/Login");
                return false;
            }

            if (!http.User.IsInRole("Supervisor"))
            {
                http.Response.Redirect("/Home/AccessDenied");
                //return false;
            }

            return true;
        }
    }
}
