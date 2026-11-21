
namespace StockPilot.Domain.Constants;

public static class SystemRoles
{
    public const string Owner = "Owner";
    public const string SystemAdmin = "SystemAdmin";
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly IReadOnlyCollection<string> All =
    [
        Owner,
        SystemAdmin,
        Manager,
        User
    ];
}
