using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Application.Features.Incidents.Security;

/// <summary>
/// Single responsibility: decide who can see or mutate incidents.
/// </summary>
public interface IIncidentAccessPolicy
{
    void EnsureAuthenticated();
    void EnsureStaff();
    bool IsStaff();
    bool IsAdmin();
    void EnsureCanView(Incident incident);
    void EnsureCanUpdateStatus(Incident incident);
    void EnsureCanAssign();
    void EnsureCanReopen(Incident incident);
    void EnsureCanManageCategories();
    IQueryable<Incident> ApplyVisibilityFilter(IQueryable<Incident> query);
}
