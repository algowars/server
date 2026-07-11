using Algowars.Application.Events;
using Algowars.Application.Services.Problems;
using Algowars.Application.Services.Users;
using Algowars.Domain.SeedWork;
using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.Factories;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.Factories;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Algowars.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly));
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceRegistration).Assembly, includeInternalTypes: true);

        services.AddFactories();
        services.AddServices();
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        services.AddScoped<UserContext>();

        return services;
    }

    private static IServiceCollection AddFactories(this IServiceCollection services)
    {
        services.AddScoped<IAggregateFactory<User, CreateUserParams>, UserFactory>();
        services.AddScoped<IAggregateFactory<Submission, CreateSubmissionParams>, SubmissionFactory>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUsernameGeneratorService, UsernameGeneratorService>();
        services.AddScoped<IProblemService, ProblemService>();

        return services;
    }
}