using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mova.Application.Authentication.Handlers;
using Mova.Application.Authentication.Validators;
using Mova.Application.Complexes.Handlers;
using Mova.Application.Users.Handlers;
using Mova.Application.Courts.Handlers;

namespace Mova.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<GoogleLoginCommandValidator>();
        services.AddScoped<IGoogleLoginHandler, GoogleLoginHandler>();
        services.AddScoped<ICompleteProfileHandler, CompleteProfileHandler>();
        services.AddScoped<ICreateComplexHandler, CreateComplexHandler>();
        services.AddScoped<IUpdateComplexHandler, UpdateComplexHandler>();
        services.AddScoped<IGetActiveComplexesHandler, GetActiveComplexesHandler>();
        services.AddScoped<IGetActiveComplexByIdHandler, GetActiveComplexByIdHandler>();
        services.AddScoped<IGetAllComplexesHandler, GetAllComplexesHandler>();
        services.AddScoped<IUpdateComplexStatusHandler, UpdateComplexStatusHandler>();
        services.AddScoped<ICreateCourtHandler, CreateCourtHandler>();
        services.AddScoped<IAssignCourtSportsHandler, AssignCourtSportsHandler>();
        services.AddScoped<IUpdateCourtAvailabilityRulesHandler, UpdateCourtAvailabilityRulesHandler>();
        services.AddScoped<IGetCourtAvailabilityRulesHandler, GetCourtAvailabilityRulesHandler>();
        services.AddScoped<IUpdateBusinessHoursHandler, UpdateBusinessHoursHandler>();
        services.AddScoped<IGetBusinessHoursHandler, GetBusinessHoursHandler>();

        return services;
    }
}
