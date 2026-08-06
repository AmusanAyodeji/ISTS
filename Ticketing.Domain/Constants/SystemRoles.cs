namespace Ticketing.Domain.Constants;

public static class SystemRoles
{
    public const string Staff = "Staff";
    public const string Agent = "Agent";
    public const string Manager = "Manager";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = [Staff, Agent, Manager, Admin];
}
