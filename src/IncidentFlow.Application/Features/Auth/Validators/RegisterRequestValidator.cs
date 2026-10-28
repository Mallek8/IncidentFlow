using FluentValidation;
using IncidentFlow.Application.Features.Auth.Models;
using IncidentFlow.Domain.Constants;

namespace IncidentFlow.Application.Features.Auth.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .Must(role => role is null || Roles.All.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", Roles.All)}.");
    }
}
