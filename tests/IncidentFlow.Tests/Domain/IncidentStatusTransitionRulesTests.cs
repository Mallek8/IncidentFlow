using FluentAssertions;
using IncidentFlow.Domain.Enums;
using IncidentFlow.Domain.Services;

namespace IncidentFlow.Tests.Domain;

public class IncidentStatusTransitionRulesTests
{
    [Theory]
    [InlineData(IncidentStatus.New, IncidentStatus.Assigned, true)]
    [InlineData(IncidentStatus.New, IncidentStatus.InProgress, false)]
    [InlineData(IncidentStatus.Assigned, IncidentStatus.InProgress, true)]
    [InlineData(IncidentStatus.InProgress, IncidentStatus.Resolved, true)]
    [InlineData(IncidentStatus.Resolved, IncidentStatus.Reopened, true)]
    [InlineData(IncidentStatus.Closed, IncidentStatus.Reopened, true)]
    [InlineData(IncidentStatus.Resolved, IncidentStatus.InProgress, false)]
    [InlineData(IncidentStatus.New, IncidentStatus.New, false)]
    public void CanTransition_ReturnsExpectedResult(IncidentStatus from, IncidentStatus to, bool expected)
    {
        IncidentStatusTransitionRules.CanTransition(from, to).Should().Be(expected);
    }

    [Fact]
    public void GetAllowedTargets_ForNew_ContainsAssignedAndClosed()
    {
        var targets = IncidentStatusTransitionRules.GetAllowedTargets(IncidentStatus.New);

        targets.Should().BeEquivalentTo([IncidentStatus.Assigned, IncidentStatus.Closed]);
    }
}
