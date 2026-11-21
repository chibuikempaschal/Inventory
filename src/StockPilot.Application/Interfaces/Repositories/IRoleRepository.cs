

namespace StockPilot.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoleWithCountDto>> GetAllRoles(CancellationToken cancellationToken);
    Task AddRoleAsync(Role role, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken);
    Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken);
    Task<UserRole?> GetUserRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken);
    Task RemoveUserRoleAsync(UserRole userRole, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
