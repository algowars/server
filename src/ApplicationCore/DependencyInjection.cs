using System.Reflection;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationCore(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly((assembly)));
        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IAccountAppService, AccountAppService>();
        services.AddScoped<IProblemAppService, ProblemAppService>();
        services.AddScoped<ISubmissionAppService, SubmissionAppService>();
        services.AddScoped<ICodeBuilderService, CodeBuilderService>();
        services.AddScoped<ICodeExecutionService, CodeExecutionService>();

        return services;
    }
}
