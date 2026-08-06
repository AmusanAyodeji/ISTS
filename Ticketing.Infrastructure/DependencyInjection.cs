using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Constants;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Email;
using Ticketing.Infrastructure.Identity;
using Ticketing.Infrastructure.Options;
using Ticketing.Infrastructure.Persistence.Context;
using Ticketing.Infrastructure.Persistence.Repositories;
using Ticketing.Infrastructure.Realtime;
using Ticketing.Infrastructure.Storage;

namespace Ticketing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServer =>
                {
                    sqlServer.MigrationsAssembly(
                        typeof(AppDbContext).Assembly.FullName);

                    sqlServer.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

        services.AddScoped(
            typeof(IGenericRepository<>),
            typeof(GenericRepository<>));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ISLARepository, SLARepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ITicketMessageRepository, TicketMessageRepository>();
        services.AddScoped<IFileAttachmentRepository, FileAttachmentRepository>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IJwtService, JwtTokenService>();

        services.Configure<CloudinarySettings>(
            configuration.GetSection("Cloudinary"));

        services.AddScoped<IStorageService, CloudinaryStorageService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<INotificationHubService, NotificationHubService>();

        services.AddHttpContextAccessor();

        var jwtSection = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(
            jwtSection["Key"] ?? string.Empty);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],
                        IssuerSigningKey =
                            new SymmetricSecurityKey(key)
                    };

                // Read token from query string for SignalR connections
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken =
                            context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "StaffOrAbove",
                policy => policy.RequireRole(
                    SystemRoles.Staff,
                    SystemRoles.Agent,
                    SystemRoles.Manager,
                    SystemRoles.Admin));

            options.AddPolicy(
                "AgentOrAbove",
                policy => policy.RequireRole(
                    SystemRoles.Agent,
                    SystemRoles.Manager,
                    SystemRoles.Admin));

            options.AddPolicy(
                "ManagerOrAdmin",
                policy => policy.RequireRole(
                    SystemRoles.Manager,
                    SystemRoles.Admin));

            options.AddPolicy(
                "AdminOnly",
                policy => policy.RequireRole(SystemRoles.Admin));
        });

        services.AddSignalR();

        return services;
    }
}