using FluentAssertions;
using IncidentFlow.Domain.Enums;
using IncidentFlow.Domain.Services;

namespace IncidentFlow.Tests.Domain;

public class SlaCalculatorTests
{
    [Theory]
    [InlineData(IncidentPriority.Critical, 4)]
    [InlineData(IncidentPriority.High, 8)]
    [InlineData(IncidentPriority.Medium, 24)]
    [InlineData(IncidentPriority.Low, 72)]
    public void CalculateDeadlineUtc_UsesDefaultHoursForPriority(IncidentPriority priority, int expectedHours)
    {
        var created = new DateTime(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc);

        var deadline = SlaCalculator.CalculateDeadlineUtc(created, priority);

        deadline.Should().Be(created.AddHours(expectedHours));
    }

    [Fact]
    public void IsOverdue_WhenPastDeadlineAndUnresolved_ReturnsTrue()
    {
        var deadline = new DateTime(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc);
        var now = deadline.AddMinutes(1);

        SlaCalculator.IsOverdue(deadline, resolutionDateUtc: null, now).Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_WhenResolvedBeforeDeadline_ReturnsFalse()
    {
        var deadline = new DateTime(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc);
        var resolved = deadline.AddHours(-1);

        SlaCalculator.IsOverdue(deadline, resolved, deadline.AddHours(2)).Should().BeFalse();
    }
}
