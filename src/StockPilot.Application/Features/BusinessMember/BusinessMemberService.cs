using StockPilot.Application.Interfaces;
using StockPilot.Application.Interfaces.Repositories;
using StockPilot.Application.Interfaces.Services;
using StockPilot.Domain.Authorization;
using StockPilot.Domain.Constants;
using StockPilot.Domain.Entities;

namespace StockPilot.Application.Features.Businesses;

public sealed class BusinessMemberService(
    IBusinessRepo businessRepo,
    IRoleRepository roleRepository,
    IApplicationCache cache) : IBusinessMemberService
{
    public async Task<IReadOnlyList<BusinessMemberResponse>> GetMembersAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        await GetActiveMembershipAsync(
            businessId,
            userId,
            cancellationToken);

        var members = await businessRepo.GetBusinessMembersAsync(
            businessId,
            cancellationToken);

        return members
            .Where(member => member.IsActive)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<BusinessMemberResponse> AddMemberAsync(
        Guid userId,
        Guid businessId,
        BusinessMemberRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var actorMembership = await GetActiveMembershipAsync(
            businessId,
            userId,
            cancellationToken);

        var requestedRole = NormalizeRole(request.Role);

        EnsureCanAssignRole(
            actorMembership.Role.Name,
            requestedRole);

        var email = NormalizeEmail(request.Email);

        var targetUser = await businessRepo.GetUserByEmailAsync(
            email,
            cancellationToken);

        if (targetUser is null)
            throw new KeyNotFoundException(
                "No registered user was found with this email address.");

        if (targetUser.Id == userId)
            throw new InvalidOperationException(
                "You cannot add yourself as a member.");

        var role = await roleRepository.GetRoleByNameAsync(
            requestedRole,
            cancellationToken);

        if (role is null)
            throw new InvalidOperationException(
                $"The business role '{requestedRole}' does not exist.");

        var existingMembership =
            await businessRepo.GetBusinessMemberAsync(
                businessId,
                targetUser.Id,
                cancellationToken);

        if (existingMembership is not null)
        {
            if (existingMembership.IsActive)
                throw new InvalidOperationException(
                    "This user is already a member of the business.");

            // Reactivate an existing membership instead of creating
            // a duplicate BusinessUser record.
            existingMembership.RoleId = role.Id;
            existingMembership.IsActive = true;
            existingMembership.JoinedAt = DateTime.UtcNow;

            await businessRepo.SaveChangesAsync(
                cancellationToken);

            await InvalidateBusinessCachesAsync(
                businessId,
                userId,
                targetUser.Id,
                cancellationToken);

            return MapToResponse(existingMembership);
        }

        var membership = new BusinessUser
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            UserId = targetUser.Id,
            RoleId = role.Id,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await businessRepo.AddMembershipAsync(
            membership,
            cancellationToken);

        await businessRepo.SaveChangesAsync(
            cancellationToken);

        await InvalidateBusinessCachesAsync(
            businessId,
            userId,
            targetUser.Id,
            cancellationToken);

        // The newly created entity may not have navigation properties
        // populated yet. Map using the known user and role.
        return new BusinessMemberResponse(
            membership.Id,
            targetUser.Id,
            targetUser.FullName,
            targetUser.Email,
            role.Name,
            membership.IsActive,
            membership.JoinedAt);
    }

    public async Task<BusinessMemberResponse> UpdateMemberRoleAsync(
        Guid userId,
        Guid businessId,
        Guid targetUserId,
        UpdateBusinessMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var actorMembership = await GetActiveMembershipAsync(
            businessId,
            userId,
            cancellationToken);

        var targetMembership =
            await businessRepo.GetBusinessMemberAsync(
                businessId,
                targetUserId,
                cancellationToken);

        if (targetMembership is null || !targetMembership.IsActive)
            throw new KeyNotFoundException(
                "The requested business member was not found.");

        var actorRole = actorMembership.Role.Name;
        var targetRole = targetMembership.Role.Name;
        var requestedRole = NormalizeRole(request.Role);

        EnsureCanManageMember(
            actorRole,
            targetRole);

        EnsureCanAssignRole(
            actorRole,
            requestedRole);

        if (string.Equals(
                targetRole,
                BusinessRoles.Owner,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The business Owner role cannot be changed through ordinary member management.");
        }

        if (string.Equals(
                targetRole,
                requestedRole,
                StringComparison.OrdinalIgnoreCase))
        {
            return MapToResponse(targetMembership);
        }

        var role = await roleRepository.GetRoleByNameAsync(
            requestedRole,
            cancellationToken);

        if (role is null)
            throw new InvalidOperationException(
                $"The business role '{requestedRole}' does not exist.");

        targetMembership.RoleId = role.Id;

        await businessRepo.SaveChangesAsync(
            cancellationToken);

        await InvalidateBusinessCachesAsync(
            businessId,
            userId,
            targetUserId,
            cancellationToken);

        // Update the in-memory navigation property for mapping.
        targetMembership.Role = role;

        return MapToResponse(targetMembership);
    }

    public async Task RemoveMemberAsync(
        Guid userId,
        Guid businessId,
        Guid targetUserId,
        CancellationToken cancellationToken)
    {
        var actorMembership = await GetActiveMembershipAsync(
            businessId,
            userId,
            cancellationToken);

        var targetMembership =
            await businessRepo.GetBusinessMemberAsync(
                businessId,
                targetUserId,
                cancellationToken);

        if (targetMembership is null || !targetMembership.IsActive)
            throw new KeyNotFoundException(
                "The requested business member was not found.");

        EnsureCanManageMember(
            actorMembership.Role.Name,
            targetMembership.Role.Name);

        if (string.Equals(
                targetMembership.Role.Name,
                BusinessRoles.Owner,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The business Owner cannot be removed through ordinary member management.");
        }

        if (userId == targetUserId)
            throw new InvalidOperationException(
                "You cannot remove yourself from the business using this operation.");

        targetMembership.IsActive = false;

        await businessRepo.SaveChangesAsync(
            cancellationToken);

        await InvalidateBusinessCachesAsync(
            businessId,
            userId,
            targetUserId,
            cancellationToken);
    }

    private async Task<BusinessUser> GetActiveMembershipAsync(
        Guid businessId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var membership = await businessRepo.GetMembershipAsync(
            businessId,
            userId,
            cancellationToken);

        if (membership is null)
            throw new UnauthorizedAccessException(
                "You are not an active member of this business.");

        return membership;
    }

    private static void EnsureCanManageMember(
        string actorRole,
        string targetRole)
    {
        if (!BusinessRoleHierarchy.CanManage(
                actorRole,
                targetRole))
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to manage this member.");
        }
    }

    private static void EnsureCanAssignRole(
        string actorRole,
        string requestedRole)
    {
        if (!BusinessRoleHierarchy.IsValidRole(requestedRole))
            throw new ArgumentException(
                "The requested business role is invalid.");

        if (!BusinessRoleHierarchy.CanAssignRole(
                actorRole,
                requestedRole))
        {
            throw new UnauthorizedAccessException(
                $"You do not have permission to assign the role '{requestedRole}'.");
        }
    }

    private static string NormalizeRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException(
                "A business role is required.");

        return role.Trim();
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "An email address is required.");

        return email.Trim().ToLowerInvariant();
    }

    private static BusinessMemberResponse MapToResponse(
        BusinessUser membership)
    {
        return new BusinessMemberResponse(
            membership.Id,
            membership.UserId,
            membership.User.FullName,
            membership.User.Email,
            membership.Role.Name,
            membership.IsActive,
            membership.JoinedAt);
    }

    private async Task InvalidateBusinessCachesAsync(
        Guid businessId,
        Guid actorUserId,
        Guid targetUserId,
        CancellationToken cancellationToken)
    {
        await cache.RemoveByTagAsync(
            "businesses",
            cancellationToken);

        await cache.RemoveByTagAsync(
            $"businesses:user:{actorUserId}",
            cancellationToken);

        if (actorUserId != targetUserId)
        {
            await cache.RemoveByTagAsync(
                $"businesses:user:{targetUserId}",
                cancellationToken);
        }
    }
}