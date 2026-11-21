
using StockPilot.Application.DTOs;

namespace StockPilot.Application.Interfaces;

public interface IRoleService
{
    Task<RoleAssignmentResponse> AssignRoleAsync(
        Guid currentUserId,
        Guid userId,
        string roleName,
        string? description,
        CancellationToken cancellationToken);

    Task<IList<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task RemoveRoleAsync(
        Guid currentUserId,
        Guid userId,
        string roleName,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RoleWithCountDto>> GetAllRolesAsync(
        CancellationToken cancellationToken);
}
