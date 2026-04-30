using ApplicationCore;
using ApplicationCore.Domain.Accounts;
using Asp.Versioning;
using Infrastructure;
using PublicApi;
using PublicApi.Extensions;
using PublicApi.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterAppSettings(builder.Configuration);

builder.Services.AddApplicationCore();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.RegisterAllUserAndGlobalRateLimitPolicies(typeof(Program).Assembly);

builder.Services.AddAuthTokenValidation(builder.Configuration);
builder.Services.AddRbacAuthorization();

builder.Services.AddScoped<IAccountContext, AccountContext>();
builder.Services.AddScoped<AccountContextMiddleware>();
builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
});

builder.Services.AddMediatR(cfg =>
{
    cfg.LicenseKey = builder.Configuration.GetSection("MediatRSettings:LicenseKey").Get<string>();
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddOpenApi();

string[] allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseAccountContext();
app.MapControllers();

app.Run();
