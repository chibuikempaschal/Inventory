
using StockPilot.Application.DTOs;
using StockPilot.Application.Interfaces;
using StockPilot.Application.Interfaces.Repositories;
using StockPilot.Application.Interfaces.Services;
using StockPilot.Domain.Authorization;
using StockPilot.Domain.Constants;
using StockPilot.Domain.Entities;
using static StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Application.Features.Roles;

public sealed class RoleService(
    IRoleRepository roleRepository,
    IApplicationCache cache) : IRoleService
{
    private static class CacheTags
    {
        public const string Users = "users";
        public const string Roles = "roles";

        public static string UserRoles(Guid userId)
            => $"user-roles:{userId}";
    }

    private static class CacheKeys
    {
        public const string AllRoles = "Roles:All";

        public static string UserRoles(Guid userId)
            => $"Roles:User:{userId}";
    }

    public async Task<RoleAssignmentResponse> AssignRoleAsync(
        Guid currentUserId,
        Guid userId,
        string roleName,
        string? description,
        CancellationToken cancellationToken)
    {
        ValidateUserIds(currentUserId, userId);

        if (currentUserId == userId)
            throw new ForbiddenException(
                "You cannot assign roles to yourself.");

        var normalizedRoleName = NormalizeRole(roleName);
        description = description?.Trim();

        await EnsureUserExistsAsync(currentUserId, cancellationToken);
        await EnsureUserExistsAsync(userId, cancellationToken);

        // Always retrieve the actor's actual roles from the database.
        var actorRoles = await GetUserRolesInternalAsync(
            currentUserId, cancellationToken);

        EnsureCanManageRole(actorRoles, normalizedRoleName);

        var previousRoles = (
            await GetUserRolesInternalAsync(userId, cancellationToken)
        ).ToList();

        EnsureCanManageTarget(actorRoles, previousRoles);

        var role = await roleRepository.GetRoleByNameAsync(
            normalizedRoleName, cancellationToken);

        if (role is null)
        {
            role = new Role
            {
                Name = normalizedRoleName,
                Description = description
            };

            await roleRepository.AddRoleAsync(
                role, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(description) &&
                 role.Description != description)
        {
            role.Description = description;
        }

        var currentRoles = previousRoles.ToList();

        if (!currentRoles.Contains(
                normalizedRoleName,
                StringComparer.OrdinalIgnoreCase))
        {
            await roleRepository.AddUserRoleAsync(
                new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id
                },
                cancellationToken);

            currentRoles.Add(normalizedRoleName);
        }

        await roleRepository.SaveChangesAsync(cancellationToken);

        // Invalidate only after the database operation succeeds.
        await InvalidateRoleCachesAsync(
            userId, cancellationToken);

        return new RoleAssignmentResponse(
            PreviousRoles: previousRoles,
            CurrentRoles: currentRoles,
            AssignedRole: normalizedRoleName,
            RoleDescription: role.Description);
    }

    public async Task RemoveRoleAsync(
        Guid currentUserId,
        Guid userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        ValidateUserIds(currentUserId, userId);

        if (currentUserId == userId)
            throw new ForbiddenException(
                "You cannot remove roles from yourself.");

        var normalizedRoleName = NormalizeRole(roleName);

        await EnsureUserExistsAsync(currentUserId, cancellationToken);
        await EnsureUserExistsAsync(userId, cancellationToken);

        // Always retrieve current authorization data from the database.
        var actorRoles = await GetUserRolesInternalAsync(
            currentUserId, cancellationToken);

        EnsureCanManageRole(actorRoles, normalizedRoleName);

        var targetRoles = await GetUserRolesInternalAsync(
            userId, cancellationToken);

        EnsureCanManageTarget(actorRoles, targetRoles);

        var assignment = await roleRepository.GetUserRoleAsync(
            userId, normalizedRoleName, cancellationToken);

        if (assignment is null)
            return;

        await roleRepository.RemoveUserRoleAsync(
            assignment, cancellationToken);

        await roleRepository.SaveChangesAsync(cancellationToken);

        await InvalidateRoleCachesAsync(
            userId, cancellationToken);
    }

    public async Task<IList<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "A valid user ID is required.", nameof(userId));

        return await cache.GetOrCreateAsync(
            CacheKeys.UserRoles(userId),
            async cancel =>
            {
                await EnsureUserExistsAsync(userId, cancel);

                return (await roleRepository.GetUserRoleNamesAsync(
                    userId, cancel)).ToList();
            },
            tags:
            [
                CacheTags.Users,
                CacheTags.UserRoles(userId)
            ],
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<RoleWithCountDto>> GetAllRolesAsync(
        CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(
            CacheKeys.AllRoles,
            async cancel =>
                await roleRepository.GetAllRoles(cancel),
            tags: [CacheTags.Roles],
            cancellationToken: cancellationToken);
    }

    private async Task EnsureUserExistsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (!await roleRepository.UserExistsAsync(
                userId, cancellationToken))
        {
            throw new KeyNotFoundException(
                $"User '{userId}' not found.");
        }
    }

    private async Task<IReadOnlyList<string>> GetUserRolesInternalAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Deliberately bypass cache for authorization decisions.
        return await roleRepository.GetUserRoleNamesAsync(
            userId, cancellationToken);
    }

    private async Task InvalidateRoleCachesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await cache.RemoveByTagAsync(
            CacheTags.UserRoles(userId), cancellationToken);

        await cache.RemoveByTagAsync(
            CacheTags.Users, cancellationToken);

        await cache.RemoveByTagAsync(
            CacheTags.Roles, cancellationToken);
    }

    private static void ValidateUserIds(
        Guid currentUserId,
        Guid userId)
    {
        if (currentUserId == Guid.Empty)
            throw new ArgumentException(
                "A valid current user ID is required.",
                nameof(currentUserId));

        if (userId == Guid.Empty)
            throw new ArgumentException(
                "A valid target user ID is required.",
                nameof(userId));
    }

    private static string NormalizeRole(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name is required.");

        var normalized = SystemRoles.All.FirstOrDefault(
            role => string.Equals(
                role,
                roleName.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (normalized is null)
            throw new ArgumentException(
                $"'{roleName}' is not a valid system role.");

        return normalized;
    }

    private static void EnsureCanManageRole(
        IReadOnlyList<string> actorRoles,
        string targetRole)
    {
        if (!actorRoles.Any(actorRole =>
                RoleHierarchy.CanManage(actorRole, targetRole)))
        {
            throw new ForbiddenException(
                $"You do not have permission to manage the '{targetRole}' role.");
        }
    }

    private static void EnsureCanManageTarget(
        IReadOnlyList<string> actorRoles,
        IReadOnlyList<string> targetRoles)
    {
        foreach (var targetRole in targetRoles)
        {
            if (!actorRoles.Any(actorRole =>
                    RoleHierarchy.CanManage(actorRole, targetRole)))
            {
                throw new ForbiddenException(
                    $"You do not have permission to manage an account with the '{targetRole}' role.");
            }
        }
    }
}
