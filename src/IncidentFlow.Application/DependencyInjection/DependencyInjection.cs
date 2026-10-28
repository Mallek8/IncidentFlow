using System.Reflection;
using FluentValidation;
using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Features.Categories;
using IncidentFlow.Application.Features.Dashboard;
using IncidentFlow.Application.Features.Incidents.Mapping;
using IncidentFlow.Application.Features.Incidents.Security;
using IncidentFlow.Application.Features.Incidents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentFlow.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Focused application services (ISP / SRP)
        services.AddScoped<IIncidentAccessPolicy, IncidentAccessPolicy>();
        services.AddScoped<IIncidentMapper, IncidentMapper>();
        services.AddScoped<ITicketNumberGenerator, TicketNumberGenerator>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
