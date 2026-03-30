using ApplicationCore;
using Asp.Versioning;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterAppSettings(builder.Configuration);

builder.Services.AddApplicationCore();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();
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
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

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
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
