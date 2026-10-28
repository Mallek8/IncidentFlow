namespace IncidentFlow.Application.Features.Incidents.Services;

/// <summary>
/// Single responsibility: generate unique incident ticket numbers.
/// </summary>
public interface ITicketNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
