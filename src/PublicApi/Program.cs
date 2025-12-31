using ApplicationCore;
using Asp.Versioning;
using Infrastructure;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using PublicApi.Authorization;
using PublicApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationCore();
builder.Services.AddInfrastructure(builder.Configuration);

ConfigureOptions(builder.Services, builder.Configuration);
AddApplicationServices(builder.Services);
AddApiVersioning(builder.Services);
AddAuthenticationAndAuthorization(builder.Services, builder.Configuration);
ConfigureCors(builder.Services, builder.Configuration.GetSection("Cors").Get<CorsOptions>()!);

var app = builder.Build();

ConfigureOpenApi(app);
ConfigureMiddleware(app);

app.MapControllers();
app.Run();

void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<Auth0Options>(configuration.GetSection("Auth0"));
    services.Configure<CorsOptions>(configuration.GetSection("Cors"));
}

void AddApplicationServices(IServiceCollection services)
{
    services.AddApplicationCore();
    services.AddControllers();
    services.AddOpenApi();
}

void AddApiVersioning(IServiceCollection services)
{
    services.AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ReportApiVersions = true;
    });
}

void AddAuthenticationAndAuthorization(IServiceCollection services, IConfiguration configuration)
{
    var auth0 = configuration.GetSection("Auth0").Get<Auth0Options>()!;

    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://{auth0.Domain}/";
            options.Audience = auth0.Audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
            };
        });

    services.AddAuthorization(options =>
    {
        options.AddPolicy(
            "create:problems",
            policy => policy.Requirements.Add(new RbacRequirement("create:problems"))
        );
        options.AddPolicy(
            "read:admin-problems",
            policy => policy.Requirements.Add(new RbacRequirement("read:admin-problems"))
        );
        options.AddPolicy(
            "read:admin-problem",
            policy => policy.Requirements.Add(new RbacRequirement("read:admin-problem"))
        );
        options.AddPolicy(
            "read:languages",
            policy => policy.Requirements.Add(new RbacRequirement("read:languages"))
        );
    });

    services.AddSingleton<IAuthorizationHandler, RbacHandler>();
}

void ConfigureCors(IServiceCollection services, CorsOptions corsOptions)
{
    if (corsOptions.AllowedOrigins.Length == 0)
    {
        throw new InvalidOperationException("CORS: No AllowedOrigins configured.");
    }

    services.AddCors(options =>
    {
        options.AddPolicy(
            "AllowFrontend",
            policy =>
            {
                policy
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
        );
    });
}

void ConfigureOpenApi(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
}

void ConfigureMiddleware(WebApplication app)
{
    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseGlobalExceptionHandler();
    app.UseAuthorization();
}
