using FluentValidation;
using IncidentFlow.Application.Features.Incidents.Models;

namespace IncidentFlow.Application.Features.Incidents.Validators;

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
    }
}
