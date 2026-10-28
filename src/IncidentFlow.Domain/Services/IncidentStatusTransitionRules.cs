using IncidentFlow.Domain.Enums;

namespace IncidentFlow.Domain.Services;

/// <summary>
/// Encodes allowed incident status transitions.
/// </summary>
public static class IncidentStatusTransitionRules
{
    private static readonly Dictionary<IncidentStatus, HashSet<IncidentStatus>> AllowedTransitions = new()
    {
        [IncidentStatus.New] = [IncidentStatus.Assigned, IncidentStatus.Closed],
        [IncidentStatus.Assigned] = [IncidentStatus.InProgress, IncidentStatus.Closed],
        [IncidentStatus.InProgress] = [IncidentStatus.Resolved, IncidentStatus.Assigned, IncidentStatus.Closed],
        [IncidentStatus.Resolved] = [IncidentStatus.Closed, IncidentStatus.Reopened],
        [IncidentStatus.Closed] = [IncidentStatus.Reopened],
        [IncidentStatus.Reopened] = [IncidentStatus.Assigned, IncidentStatus.InProgress, IncidentStatus.Closed]
    };

    public static bool CanTransition(IncidentStatus from, IncidentStatus to)
    {
        if (from == to)
        {
            return false;
        }

        return AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
    }

    public static IReadOnlyCollection<IncidentStatus> GetAllowedTargets(IncidentStatus from)
    {
        return AllowedTransitions.TryGetValue(from, out var targets)
            ? targets.ToArray()
            : Array.Empty<IncidentStatus>();
    }
}
