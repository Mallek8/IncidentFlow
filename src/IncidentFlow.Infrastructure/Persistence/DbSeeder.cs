using IncidentFlow.Domain.Constants;
using IncidentFlow.Domain.Entities;
using IncidentFlow.Domain.Enums;
using IncidentFlow.Domain.Services;
using IncidentFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IncidentFlow.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        if (db.Database.IsSqlite())
        {
            // Local development without SQL Server: create schema from the current model.
            await db.Database.EnsureCreatedAsync();
        }
        else
        {
            await db.Database.MigrateAsync();
        }

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await SeedCategoriesAsync(db);
        await SeedSlaPoliciesAsync(db);
        await SeedUsersAsync(userManager, logger);
        await SeedSampleIncidentsAsync(db, userManager, logger);
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext db)
    {
        if (await db.IncidentCategories.AnyAsync())
        {
            return;
        }

        db.IncidentCategories.AddRange(
            new IncidentCategory { Name = "Hardware", Description = "Laptops, peripherals, and physical devices" },
            new IncidentCategory { Name = "Software", Description = "Desktop and line-of-business applications" },
            new IncidentCategory { Name = "Network", Description = "Connectivity, VPN, and Wi-Fi issues" },
            new IncidentCategory { Name = "Access", Description = "Accounts, permissions, and MFA" },
            new IncidentCategory { Name = "Email", Description = "Mailbox and distribution list issues" });

        await db.SaveChangesAsync();
    }

    private static async Task SeedSlaPoliciesAsync(ApplicationDbContext db)
    {
        if (await db.SlaPolicies.AnyAsync())
        {
            return;
        }

        foreach (var (priority, hours) in SlaCalculator.DefaultResolutionHours)
        {
            db.SlaPolicies.Add(new SlaPolicy
            {
                Priority = priority,
                ResolutionTimeHours = hours,
                Name = $"{priority} SLA",
                Description = $"Resolve {priority} incidents within {hours} calendar hours (24/7 demo policy).",
                IsActive = true
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        await EnsureUserAsync(
            userManager,
            email: "admin@incidentflow.local",
            password: "Admin123!",
            firstName: "Alex",
            lastName: "Admin",
            role: Roles.Admin,
            logger);

        await EnsureUserAsync(
            userManager,
            email: "tech1@incidentflow.local",
            password: "Tech123!",
            firstName: "Taylor",
            lastName: "Technician",
            role: Roles.Technician,
            logger);

        await EnsureUserAsync(
            userManager,
            email: "tech2@incidentflow.local",
            password: "Tech123!",
            firstName: "Jordan",
            lastName: "Support",
            role: Roles.Technician,
            logger);

        await EnsureUserAsync(
            userManager,
            email: "employee@incidentflow.local",
            password: "Employee123!",
            firstName: "Casey",
            lastName: "Employee",
            role: Roles.Employee,
            logger);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        ILogger logger)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to seed user {Email}: {Errors}",
                email, string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, role);
        logger.LogInformation("Seeded development user {Email} with role {Role}", email, role);
    }

    private static async Task SeedSampleIncidentsAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        if (await db.Incidents.AnyAsync())
        {
            return;
        }

        var employee = await userManager.FindByEmailAsync("employee@incidentflow.local");
        var tech = await userManager.FindByEmailAsync("tech1@incidentflow.local");
        var category = await db.IncidentCategories.FirstOrDefaultAsync(c => c.Name == "Software");

        if (employee is null || tech is null || category is null)
        {
            logger.LogWarning("Skipping sample incidents seed because required users/categories are missing.");
            return;
        }

        var created = DateTime.UtcNow.AddHours(-6);
        var incident = new Incident
        {
            TicketNumber = "INC-100001",
            Title = "Cannot launch payroll application",
            Description = "The payroll desktop client crashes immediately after login on Windows 11.",
            CategoryId = category.Id,
            Priority = IncidentPriority.High,
            Status = IncidentStatus.Assigned,
            CreatedByUserId = employee.Id,
            AssignedTechnicianId = tech.Id,
            CreatedAtUtc = created,
            SlaDeadlineUtc = SlaCalculator.CalculateDeadlineUtc(created, IncidentPriority.High)
        };

        incident.Assignments.Add(new IncidentAssignment
        {
            TechnicianUserId = tech.Id,
            AssignedByUserId = tech.Id,
            AssignedAtUtc = created.AddMinutes(30),
            IsActive = true,
            Notes = "Initial assignment for development seed data."
        });

        incident.History.Add(new IncidentHistory
        {
            ChangedByUserId = employee.Id,
            ChangeType = "Created",
            NewStatus = IncidentStatus.New,
            ChangedAtUtc = created,
            Notes = "Incident created by employee (seed)."
        });

        incident.History.Add(new IncidentHistory
        {
            ChangedByUserId = tech.Id,
            ChangeType = "StatusChanged",
            OldStatus = IncidentStatus.New,
            NewStatus = IncidentStatus.Assigned,
            ChangedAtUtc = created.AddMinutes(30),
            Notes = "Assigned to technician (seed)."
        });

        incident.Comments.Add(new IncidentComment
        {
            AuthorUserId = employee.Id,
            Content = "Happens every morning after the latest Windows update.",
            IsInternal = false,
            CreatedAtUtc = created.AddMinutes(5)
        });

        incident.Comments.Add(new IncidentComment
        {
            AuthorUserId = tech.Id,
            Content = "Checking event viewer logs and recent patch history.",
            IsInternal = true,
            CreatedAtUtc = created.AddHours(1)
        });

        db.Incidents.Add(incident);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded sample incident {Ticket}", incident.TicketNumber);
    }
}