using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.OpenApi.Models;
using Serilog;
using Ticketing.Application.Common.Mappings;
using Ticketing.API.Middleware;
using Ticketing.Application;
using Ticketing.Domain.Entities;
using MassTransit;
using Ticketing.Infrastructure;
using Ticketing.Infrastructure.Persistence.Context;
using Ticketing.Infrastructure.Persistence.Seeders;
using Ticketing.Infrastructure.Realtime;
using Swashbuckle.AspNetCore.Filters;
using Ticketing.Api.SwaggerExamples;
using Ticketing.Infrastructure.BackgroundJobs;
using Ticketing.Infrastructure.Consumer;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/ticketing-.log", rollingInterval: RollingInterval.Day);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

const string CorsPolicyName = "AllowFrontend";

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins.Any())
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.ExampleFilters();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Internal Staff Support Ticketing API",
        Version = "v1"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Input your JWT token in this format: Bearer {token}",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<CreateSLARequestDTOExample>();

builder.Services.AddHostedService<SLABreachBackgroundService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<JobConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration["RabbitMQ:ConnectionString"]!));
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<RequestContextMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Redirect root URL to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Trust forwarded headers when behind a reverse proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

// Enable HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    ContentTypeProvider = new FileExtensionContentTypeProvider()
});

app.UseStaticFiles();

// Enable CORS
app.UseCors(CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<SupportHub>("/hubs/support");
app.MapHub<NotificationHub>("/hubs/notifications");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await DataSeeder.SeedAsync(db, hasher, seederLogger);
}

app.Run();