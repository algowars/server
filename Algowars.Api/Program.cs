using Algowars.Api;
using Algowars.Api.Middleware;
using Algowars.Application;
using Algowars.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.UseApi();

app.UseGlobalExceptionHandler();
app.Run();