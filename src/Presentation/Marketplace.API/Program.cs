using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Marketplace.API.Authorization;
using Marketplace.API.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Marketplace.Application.Authentication.Commands.LoginUser;
using Marketplace.Identity;
using Marketplace.Identity.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Marketplace API host with Auth Service...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://marketplace.seq:80"));

    // Register Services
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
            {
                new() { Url = "/" }
            };
            return Task.CompletedTask;
        });
    });
    builder.Services.AddControllers();
    builder.Services.AddSignalR();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddCors(options =>
    { 
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Register Infrastructure & Application Layers
    builder.Services.AddIdentityInfrastructure(builder.Configuration);

    // Register MediatR & FluentValidation
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(Marketplace.Application.Common.Behaviors.ValidationBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(Marketplace.Application.Common.Behaviors.TransactionBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(LoginCommand).Assembly);

    // JWT Authentication Setup
    var secretKey = builder.Configuration["JwtSettings:SecretKey"] 
        ?? "SuperSecretKeyForMarketplaceSecurityService2026!";
    var issuer = builder.Configuration["JwtSettings:Issuer"] ?? "MarketplaceAPI";
    var audience = builder.Configuration["JwtSettings:Audience"] ?? "MarketplaceClients";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
    
    builder.Services.AddAuthorization();
    builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
    builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

    // Localization Setup (Trilingual: English, Dari/Persian, Pashto)
    var supportedCultures = new[] { "en", "prs", "fa", "ps" };
    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        options.SetDefaultCulture("en")
               .AddSupportedCultures(supportedCultures)
               .AddSupportedUICultures(supportedCultures);
    });

    // Rate Limiter Configuration (OWASP Anti-Brute Force)
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("LoginLimiter", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
    });

    var app = builder.Build();

    app.UseRequestLocalization();

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<Marketplace.Identity.Persistence.ApplicationDbContext>();
        context.Database.Migrate();
        await RbacSeeder.SeedAsync(scope.ServiceProvider);
    }
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Marketplace API - Auth Service")
                   .WithTheme(ScalarTheme.Purple);
        });
    }

    app.UseCors("AllowAll");

    var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    if (!Directory.Exists(wwwrootPath))
    {
        Directory.CreateDirectory(wwwrootPath);
    }
    var uploadsPath = Path.Combine(wwwrootPath, "uploads");
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(wwwrootPath),
        RequestPath = ""
    });

    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<Marketplace.API.Hubs.NotificationHub>("/hubs/notifications");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Marketplace API host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
