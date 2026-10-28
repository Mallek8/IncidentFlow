using FluentValidation;
using IncidentFlow.Application.Features.Incidents.Models;
using IncidentFlow.Domain.Enums;

namespace IncidentFlow.Application.Features.Incidents.Validators;

public class UpdateIncidentStatusRequestValidator : AbstractValidator<UpdateIncidentStatusRequest>
{
    public UpdateIncidentStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.ResolutionNotes)
            .NotEmpty()
            .MaximumLength(4000)
            .When(x => x.Status == IncidentStatus.Resolved);
    }
}
