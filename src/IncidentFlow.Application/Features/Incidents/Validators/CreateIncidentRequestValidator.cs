using FluentValidation;
using IncidentFlow.Application.Features.Incidents.Models;

namespace IncidentFlow.Application.Features.Incidents.Validators;

public class CreateIncidentRequestValidator : AbstractValidator<CreateIncidentRequest>
{
    public CreateIncidentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Priority).IsInEnum();
    }
}
