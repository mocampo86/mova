using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mova.Application.Authentication.Handlers;
using Mova.Application.Authentication.Validators;
using Mova.Application.Complexes.Handlers;
using Mova.Application.Reservations.Handlers;
using Mova.Application.Reservations.Validators;
using Mova.Application.Users.Handlers;
using Mova.Application.Courts.Handlers;

namespace Mova.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<GoogleLoginCommandValidator>();
        services.AddScoped<IGoogleLoginHandler, GoogleLoginHandler>();
        services.AddScoped<ICompleteComplexAdminHandler, CompleteComplexAdminHandler>();
        services.AddScoped<ICompleteProfileHandler, CompleteProfileHandler>();
        services.AddScoped<ICreateComplexHandler, CreateComplexHandler>();
        services.AddScoped<IUpdateComplexHandler, UpdateComplexHandler>();
        services.AddScoped<IGetActiveComplexesHandler, GetActiveComplexesHandler>();
        services.AddScoped<IGetActiveComplexByIdHandler, GetActiveComplexByIdHandler>();
        services.AddScoped<IGetAllComplexesHandler, GetAllComplexesHandler>();
        services.AddScoped<IGetComplexDashboardHandler, GetComplexDashboardHandler>();
        services.AddScoped<IUpdateComplexStatusHandler, UpdateComplexStatusHandler>();
        services.AddScoped<ICreateCourtHandler, CreateCourtHandler>();
        services.AddScoped<IUpdateCourtHandler, UpdateCourtHandler>();
        services.AddScoped<IAssignCourtSportsHandler, AssignCourtSportsHandler>();
        services.AddScoped<IUpdateCourtStatusHandler, UpdateCourtStatusHandler>();
        services.AddScoped<IGetCourtByIdHandler, GetCourtByIdHandler>();
        services.AddScoped<IUpdateCourtAvailabilityRulesHandler, UpdateCourtAvailabilityRulesHandler>();
        services.AddScoped<IGetCourtAvailabilityRulesHandler, GetCourtAvailabilityRulesHandler>();
        services.AddScoped<IGetActiveCourtsByComplexHandler, GetActiveCourtsByComplexHandler>();
        services.AddScoped<IGetCourtsByComplexHandler, GetCourtsByComplexHandler>();
        services.AddScoped<IGetActiveCourtByIdHandler, GetActiveCourtByIdHandler>();
        services.AddScoped<IUpdateBusinessHoursHandler, UpdateBusinessHoursHandler>();
        services.AddScoped<IGetBusinessHoursHandler, GetBusinessHoursHandler>();
        services.AddScoped<IGetCourtAvailabilityHandler, GetCourtAvailabilityHandler>();
        services.AddScoped<IGetActiveSportsHandler, GetActiveSportsHandler>();

        services.AddScoped<ICreateReservationHandler, CreateReservationHandler>();
        services.AddScoped<IGetReservationByIdHandler, GetReservationByIdHandler>();
        services.AddScoped<IGetReservationsByComplexHandler, GetReservationsByComplexHandler>();
        services.AddScoped<ICancelReservationHandler, CancelReservationHandler>();
        services.AddScoped<IUpdateReservationStatusHandler, UpdateReservationStatusHandler>();
        services.AddValidatorsFromAssemblyContaining<CreateReservationCommandValidator>();

        return services;
    }
}
