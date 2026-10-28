using IncidentFlow.Domain.Enums;

namespace IncidentFlow.Application.Features.Incidents.Models;

public record CreateIncidentRequest(
    string Title,
    string Description,
    Guid CategoryId,
    IncidentPriority Priority);

public record UpdateIncidentStatusRequest(
    IncidentStatus Status,
    string? ResolutionNotes);

public record AssignIncidentRequest(
    string TechnicianUserId,
    string? Notes);

public record AddCommentRequest(
    string Content,
    bool IsInternal);

public record IncidentListItemDto(
    Guid Id,
    string TicketNumber,
    string Title,
    string CategoryName,
    IncidentPriority Priority,
    IncidentStatus Status,
    string CreatedByUserId,
    string? AssignedTechnicianId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime SlaDeadlineUtc,
    bool IsOverdue);

public record IncidentDetailDto(
    Guid Id,
    string TicketNumber,
    string Title,
    string Description,
    Guid CategoryId,
    string CategoryName,
    IncidentPriority Priority,
    IncidentStatus Status,
    string CreatedByUserId,
    string? AssignedTechnicianId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? ResolutionDateUtc,
    DateTime SlaDeadlineUtc,
    string? ResolutionNotes,
    bool IsOverdue,
    IReadOnlyList<IncidentStatus> AllowedNextStatuses,
    IReadOnlyList<IncidentCommentDto> Comments,
    IReadOnlyList<IncidentHistoryDto> History);

public record IncidentCommentDto(
    Guid Id,
    string AuthorUserId,
    string Content,
    bool IsInternal,
    DateTime CreatedAtUtc);

public record IncidentHistoryDto(
    Guid Id,
    string ChangedByUserId,
    string ChangeType,
    string? FieldName,
    string? OldValue,
    string? NewValue,
    IncidentStatus? OldStatus,
    IncidentStatus? NewStatus,
    DateTime ChangedAtUtc,
    string? Notes);

public record IncidentCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive);

public record CreateCategoryRequest(
    string Name,
    string? Description);

public record DashboardSummaryDto(
    int TotalIncidents,
    int OpenIncidents,
    int CriticalIncidents,
    int ResolvedIncidents,
    double? AverageResolutionHours,
    double SlaComplianceRate,
    IReadOnlyList<NamedCountDto> ByCategory,
    IReadOnlyList<NamedCountDto> ByPriority,
    IReadOnlyList<MonthlyTrendDto> MonthlyTrends);

public record NamedCountDto(string Name, int Count);

public record MonthlyTrendDto(string Month, int Count);

public class IncidentQuery : Common.Models.PaginationQuery
{
    public IncidentStatus? Status { get; set; }
    public IncidentPriority? Priority { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? OverdueOnly { get; set; }
    public bool AssignedToMe { get; set; }
    public bool CreatedByMe { get; set; }
}
