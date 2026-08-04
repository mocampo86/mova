using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mova.Application.Authentication.Handlers;
using Mova.Application.Authentication.Validators;

namespace Mova.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<GoogleLoginCommandValidator>();
        services.AddScoped<IGoogleLoginHandler, GoogleLoginHandler>();

        return services;
    }
}
