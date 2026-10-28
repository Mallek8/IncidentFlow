namespace IncidentFlow.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "IncidentFlow";
    public string Audience { get; set; } = "IncidentFlow";
    public string Key { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
