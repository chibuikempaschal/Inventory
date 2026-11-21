
namespace StockPilot.Domain.Constants;

public static class BusinessRoles
{
    public const string Owner = "Owner";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";
    public const string Staff = "Staff";

    public static readonly IReadOnlyCollection<string> All =
    [
        Owner,
        Manager,
        Cashier,
        Staff
    ];
}
