using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IncidentFlow.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations (dotnet ef).
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../IncidentFlow.API"));
        if (!Directory.Exists(basePath))
        {
            basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "src/IncidentFlow.API"));
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1433;Database=IncidentFlowDb;User Id=sa;Password=CHANGE_ME_LocalDev_Only!;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
