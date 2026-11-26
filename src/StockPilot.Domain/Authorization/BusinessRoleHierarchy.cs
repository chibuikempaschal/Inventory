using StockPilot.Domain.Constants;

namespace StockPilot.Domain.Authorization;

public static class BusinessRoleHierarchy
{
    // Higher number means higher business privilege.
    private static readonly IReadOnlyDictionary<string, int> RoleRanks =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [BusinessRoles.Owner] = 100,
            [BusinessRoles.Manager] = 80,
            [BusinessRoles.Cashier] = 50,
            [BusinessRoles.Staff] = 20
        };

    public static bool IsValidRole(string? role)
    {
        return !string.IsNullOrWhiteSpace(role) &&
               RoleRanks.ContainsKey(role.Trim());
    }

    public static int GetRank(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException(
                "Business role cannot be empty.",
                nameof(role));
        }

        if (!RoleRanks.TryGetValue(role.Trim(), out var rank))
        {
            throw new ArgumentException(
                $"'{role}' is not a valid business role.",
                nameof(role));
        }

        return rank;
    }

    public static bool IsAtLeast(
        string actorRole,
        string requiredRole)
    {
        return GetRank(actorRole) >= GetRank(requiredRole);
    }

    public static bool CanManage(
        string actorRole,
        string targetRole)
    {
        if (!IsValidRole(actorRole) ||
            !IsValidRole(targetRole))
        {
            return false;
        }

        // Nobody can manage or replace the Owner
        // through ordinary membership management.
        if (string.Equals(
            targetRole,
            BusinessRoles.Owner,
            StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // An Owner cannot manage another Owner through
        // ordinary membership management either.
        return GetRank(actorRole) > GetRank(targetRole);
    }

    public static bool CanAssignRole(
        string actorRole,
        string requestedRole)
    {
        if (!IsValidRole(actorRole) ||
            !IsValidRole(requestedRole))
        {
            return false;
        }

        // Only the Owner can assign Manager, Cashier or Staff.
        if (string.Equals(
            requestedRole,
            BusinessRoles.Manager,
            StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(
                actorRole,
                BusinessRoles.Owner,
                StringComparison.OrdinalIgnoreCase);
        }

        // Owner and Manager can assign Cashier or Staff.
        if (string.Equals(
            requestedRole,
            BusinessRoles.Cashier,
            StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                requestedRole,
                BusinessRoles.Staff,
                StringComparison.OrdinalIgnoreCase))
        {
            return IsAtLeast(actorRole, BusinessRoles.Manager);
        }

        // Owner cannot be assigned through normal role management.
        return false;
    }
}