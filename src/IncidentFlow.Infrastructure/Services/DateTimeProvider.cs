using IncidentFlow.Application.Common.Interfaces;

namespace IncidentFlow.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
