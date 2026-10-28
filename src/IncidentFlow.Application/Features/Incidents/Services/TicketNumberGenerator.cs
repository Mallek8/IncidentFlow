using IncidentFlow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Application.Features.Incidents.Services;

public class TicketNumberGenerator : ITicketNumberGenerator
{
    private readonly IApplicationDbContext _db;

    public TicketNumberGenerator(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var count = await _db.Incidents.CountAsync(cancellationToken);
        return $"INC-{100001 + count}";
    }
}