using System.Reflection;
using FluentValidation;
using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Features.Incidents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentFlow.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IIncidentService, IncidentService>();
        return services;
    }
}
