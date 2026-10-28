namespace IncidentFlow.Domain.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Technician = "Technician";
    public const string Employee = "Employee";

    public static readonly string[] All = [Admin, Technician, Employee];
}
