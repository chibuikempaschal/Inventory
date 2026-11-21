
using StockPilot.Domain.Constants;

namespace StockPilot.Domain.Authorization;

public static class RoleHierarchy
{
    private static readonly IReadOnlyDictionary<string, int> Rank =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [SystemRoles.Owner] = 100,
            [SystemRoles.SystemAdmin] = 80,
            [SystemRoles.Manager] = 60,
            [SystemRoles.User] = 20
        };

    public static bool IsValidRole(string? role)
    {
        return !string.IsNullOrWhiteSpace(role) &&
               Rank.ContainsKey(role.Trim());
    }

    public static int GetRank(string role)
    {
        if (string.IsNullOrWhiteSpace(role) ||
            !Rank.TryGetValue(role.Trim(), out var rank))
        {
            throw new ArgumentException(
                $"Unknown system role: '{role}'.",
                nameof(role));
        }

        return rank;
    }

    public static bool IsAtLeast(string role, string requiredRole)
    {
        return GetRank(role) >= GetRank(requiredRole);
    }

    public static bool CanManage(string managerRole, string targetRole)
    {
        if (!IsValidRole(managerRole) || !IsValidRole(targetRole))
            return false;

        // Owner is the only role allowed to manage SystemAdmin.
        if (string.Equals(
            targetRole,
            SystemRoles.SystemAdmin,
            StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(
                managerRole,
                SystemRoles.Owner,
                StringComparison.OrdinalIgnoreCase);
        }

        // No role can manage an equal or higher role.
        return GetRank(managerRole) > GetRank(targetRole);
    }
}
