using System.Reflection;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Mappings;
using ApplicationCore.Services;
using FluentValidation;
using Mapster;
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
        services.AddScoped<IExecutionComparisonService, ExecutionComparisonService>();

        TypeAdapterConfig.GlobalSettings.Scan(typeof(ProblemMappings).Assembly);

        return services;
    }
}
