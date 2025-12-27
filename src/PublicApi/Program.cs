using ApplicationCore;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationCore();
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void AddAuthentication(IServiceCollection services)
{
    builder
        .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
            options.Audience = builder.Configuration["Auth0:Audience"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(
            "create:problems",
            policy =>
            {
                policy.Requirements.Add(new RbacRequirement("create:problems"));
            }
        );

        options.AddPolicy(
            "read:admin-problems",
            policy =>
            {
                policy.Requirements.Add(new RbacRequirement("read:admin-problems"));
            }
        );
        options.AddPolicy(
            "read:admin-problem",
            policy =>
            {
                policy.Requirements.Add(new RbacRequirement("read:admin-problem"));
            }
        );
        options.AddPolicy(
            "read:languages",
            policy => policy.Requirements.Add(new RbacRequirement("read:languages"))
        );
    });

    builder.Services.AddSingleton<IAuthorizationHandler, RbacHandler>();
}
