using Algowars.Api.Context;
using Algowars.Api.Extensions;
using Algowars.Api.Middleware;
using Algowars.Application;
using Algowars.Infrastructure;
using Algowars.Infrastructure.Persistence;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();
builder.Services.AddApplication();

builder.Services.AddControllers();

builder.Services.AddApiRateLimiting(typeof(Program).Assembly);

builder.Services.AddAuth0Authentication(builder.Configuration);
builder.Services.AddRbacAuthorization();

builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<UserContextMiddleware>();

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
});

builder.Services.AddOpenApi();

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithOpenApiRoutePattern("/openapi/{documentName}.json"));
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors();
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseUserContext();
app.MapControllers();
app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AlgoWarsDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();
