using IncidentFlow.Domain.Enums;

namespace IncidentFlow.Domain.Services;

/// <summary>
/// Calculates SLA deadlines from creation time and priority policies.
/// Demo defaults use calendar hours (24/7), not business-hours calendars.
/// Documented in README; can be replaced with business-hours logic later.
/// </summary>
public static class SlaCalculator
{
    public static readonly IReadOnlyDictionary<IncidentPriority, int> DefaultResolutionHours =
        new Dictionary<IncidentPriority, int>
        {
            [IncidentPriority.Critical] = 4,
            [IncidentPriority.High] = 8,
            [IncidentPriority.Medium] = 24,
            [IncidentPriority.Low] = 72
        };

    public static DateTime CalculateDeadlineUtc(
        DateTime createdAtUtc,
        IncidentPriority priority,
        int? resolutionTimeHours = null)
    {
        var hours = resolutionTimeHours ?? DefaultResolutionHours[priority];
        return createdAtUtc.AddHours(hours);
    }

    public static bool IsOverdue(DateTime slaDeadlineUtc, DateTime? resolutionDateUtc, DateTime utcNow)
    {
        var effectiveEnd = resolutionDateUtc ?? utcNow;
        return effectiveEnd > slaDeadlineUtc;
    }

    public static double? CalculateResolutionHours(DateTime createdAtUtc, DateTime? resolutionDateUtc)
    {
        if (!resolutionDateUtc.HasValue)
        {
            return null;
        }

        return (resolutionDateUtc.Value - createdAtUtc).TotalHours;
    }
}
