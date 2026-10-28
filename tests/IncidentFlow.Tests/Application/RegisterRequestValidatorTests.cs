using FluentAssertions;
using IncidentFlow.Application.Features.Auth.Models;
using IncidentFlow.Application.Features.Auth.Validators;

namespace IncidentFlow.Tests.Application;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenValidEmployeeRegistration_Succeeds()
    {
        var request = new RegisterRequest(
            "casey@example.com",
            "Employee123!",
            "Casey",
            "Employee",
            "Employee");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenEmailMissing_Fails()
    {
        var request = new RegisterRequest("", "Employee123!", "Casey", "Employee");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WhenRoleInvalid_Fails()
    {
        var request = new RegisterRequest(
            "casey@example.com",
            "Employee123!",
            "Casey",
            "Employee",
            "SuperUser");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Role");
    }
}
