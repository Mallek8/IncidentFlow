using FluentValidation;
using IncidentFlow.Application.Features.Incidents.Models;

namespace IncidentFlow.Application.Features.Incidents.Validators;

public class AssignIncidentRequestValidator : AbstractValidator<AssignIncidentRequest>
{
    public AssignIncidentRequestValidator()
    {
        RuleFor(x => x.TechnicianUserId).NotEmpty().MaximumLength(450);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
