using Algowars.Application.Services.Users;
using Algowars.Domain.SeedWork;
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

        services.AddScoped<IAggregateFactory<User, CreateUserParams>, UserFactory>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUsernameGeneratorService, UsernameGeneratorService>();
        services.AddScoped<UserContext>();

        return services;
    }
}
