using ApplicationCore;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using PublicApi.Authorization;
using PublicApi.Middleware;

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

AddAuthentication(builder.Services);

string[] allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("CORS: No Cors:AllowedOrigins configured.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHsts();
app.UseHttpsRedirection();

// Security Headers
app.UseXContentTypeOptions(); // Prevent MIME type sniffing
app.UseReferrerPolicy(opts => opts.NoReferrer()); // Hide referrer
app.UseXXssProtection(options => options.EnabledWithBlockMode());
app.UseXfo(options => options.Deny()); // Prevent clickjacking
app.UseCsp(options =>
    options.DefaultSources(s => s.Self()).StyleSources(s => s.Self().UnsafeInline())
);

app.UseCors("AllowFrontend");

app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
return;

void AddAuthentication(IServiceCollection services)
{
    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

    services.AddAuthorization(options =>
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
