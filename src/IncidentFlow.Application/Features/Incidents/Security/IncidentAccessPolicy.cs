using IncidentFlow.Application.Common.Exceptions;
using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Domain.Constants;
using IncidentFlow.Domain.Entities;
using IncidentFlow.Domain.Enums;

namespace IncidentFlow.Application.Features.Incidents.Security;

public class IncidentAccessPolicy : IIncidentAccessPolicy
{
    private readonly ICurrentUserService _currentUser;

    public IncidentAccessPolicy(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public void EnsureAuthenticated()
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException("Authentication required.");
        }
    }

    public void EnsureStaff()
    {
        if (!IsStaff())
        {
            throw new ForbiddenAccessException("Only technicians and admins can perform this action.");
        }
    }

    public bool IsStaff() =>
        _currentUser.IsInRole(Roles.Admin) || _currentUser.IsInRole(Roles.Technician);

    public bool IsAdmin() => _currentUser.IsInRole(Roles.Admin);

    public void EnsureCanView(Incident incident)
    {
        if (IsAdmin())
        {
            return;
        }

        if (_currentUser.IsInRole(Roles.Technician))
        {
            if (incident.AssignedTechnicianId == _currentUser.UserId ||
                incident.CreatedByUserId == _currentUser.UserId ||
                incident.Status == IncidentStatus.New)
            {
                return;
            }
        }

        if (incident.CreatedByUserId == _currentUser.UserId)
        {
            return;
        }

        throw new ForbiddenAccessException("You are not allowed to view this incident.");
    }

    public void EnsureCanUpdateStatus(Incident incident)
    {
        EnsureStaff();

        if (_currentUser.IsInRole(Roles.Technician) &&
            !IsAdmin() &&
            incident.AssignedTechnicianId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("Technicians can only update incidents assigned to them.");
        }
    }

    public void EnsureCanAssign()
    {
        EnsureAuthenticated();
        if (!IsAdmin())
        {
            throw new ForbiddenAccessException("Only administrators can assign incidents.");
        }
    }

    public void EnsureCanReopen(Incident incident)
    {
        EnsureAuthenticated();
        var isOwner = incident.CreatedByUserId == _currentUser.UserId;
        if (!isOwner && !IsAdmin())
        {
            throw new ForbiddenAccessException("Only the reporter or an admin can reopen an incident.");
        }
    }

    public void EnsureCanManageCategories()
    {
        EnsureAuthenticated();
        if (!IsAdmin())
        {
            throw new ForbiddenAccessException("Only administrators can manage categories.");
        }
    }

    public IQueryable<Incident> ApplyVisibilityFilter(IQueryable<Incident> query)
    {
        if (IsAdmin())
        {
            return query;
        }

        if (_currentUser.IsInRole(Roles.Technician))
        {
            return query.Where(i =>
                i.AssignedTechnicianId == _currentUser.UserId ||
                i.CreatedByUserId == _currentUser.UserId ||
                i.Status == IncidentStatus.New);
        }

        return query.Where(i => i.CreatedByUserId == _currentUser.UserId);
    }
}
