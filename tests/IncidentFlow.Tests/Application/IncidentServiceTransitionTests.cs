using FluentAssertions;
using IncidentFlow.Application.Common.Exceptions;
using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Features.Incidents.Models;
using IncidentFlow.Application.Features.Incidents.Services;
using IncidentFlow.Domain.Constants;
using IncidentFlow.Domain.Entities;
using IncidentFlow.Domain.Enums;
using IncidentFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IncidentFlow.Tests.Application;

public class IncidentServiceTransitionTests
{
    [Fact]
    public async Task UpdateStatusAsync_WhenInvalidTransition_ThrowsBusinessRuleException()
    {
        await using var db = CreateDb();
        var category = new IncidentCategory { Name = "Software" };
        db.IncidentCategories.Add(category);

        var incident = new Incident
        {
            TicketNumber = "INC-100001",
            Title = "Test",
            Description = "Desc",
            CategoryId = category.Id,
            Category = category,
            Priority = IncidentPriority.Medium,
            Status = IncidentStatus.New,
            CreatedByUserId = "user-1",
            AssignedTechnicianId = "tech-1",
            SlaDeadlineUtc = DateTime.UtcNow.AddHours(24)
        };
        db.Incidents.Add(incident);
        await db.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUser.SetupGet(x => x.UserId).Returns("tech-1");
        currentUser.Setup(x => x.IsInRole(Roles.Technician)).Returns(true);
        currentUser.Setup(x => x.IsInRole(Roles.Admin)).Returns(false);

        var clock = new Mock<IDateTimeProvider>();
        clock.SetupGet(x => x.UtcNow).Returns(DateTime.UtcNow);

        var service = new IncidentService(db, currentUser.Object, clock.Object);

        var act = async () => await service.UpdateStatusAsync(
            incident.Id,
            new UpdateIncidentStatusRequest(IncidentStatus.Resolved, "done"));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}