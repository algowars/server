using System.Threading.RateLimiting;
using ApplicationCore;
using Asp.Versioning;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using PublicApi.Authorization;
using PublicApi.Filters;
using PublicApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine(builder.Environment.EnvironmentName);

builder.Services.AddApplicationCore();
builder.Services.AddInfrastructure(builder.Configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers(options => options.Filters.Add<WrapResponseAttribute>());

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
});

// Authentication
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

// Authorization
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

// CORS
string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

if (allowedOrigins is null || allowedOrigins.Length == 0)
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

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        policyName: "ExtraShort",
        configureOptions: opts =>
        {
            opts.PermitLimit = 4;
            opts.Window = TimeSpan.FromSeconds(100);
            opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opts.QueueLimit = 2;
        }
    );

    options.AddFixedWindowLimiter(
        policyName: "Short",
        configureOptions: opts =>
        {
            opts.PermitLimit = 10;
            opts.Window = TimeSpan.FromSeconds(30);
            opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opts.QueueLimit = 2;
        }
    );

    options.AddFixedWindowLimiter(
        policyName: "Medium",
        configureOptions: opts =>
        {
            opts.PermitLimit = 20;
            opts.Window = TimeSpan.FromMinutes(1);
            opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opts.QueueLimit = 2;
        }
    );

    options.AddFixedWindowLimiter(
        policyName: "Long",
        configureOptions: opts =>
        {
            opts.PermitLimit = 50;
            opts.Window = TimeSpan.FromMinutes(5);
            opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opts.QueueLimit = 5;
        }
    );
});

var app = builder.Build();

app.UseRateLimiter();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
