
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.DTOs;
using StockPilot.Application.Interfaces.Repositories;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(InventoryDbContext dbContext) : IRoleRepository
{
    public Task<bool> UserExistsAsync(
        Guid userId,
        CancellationToken cancellationToken)
        => dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId, cancellationToken);

    public Task<Role?> GetRoleByNameAsync(
        string roleName,
        CancellationToken cancellationToken)
        => dbContext.Roles
            .SingleOrDefaultAsync(
                r => r.Name == roleName,
                cancellationToken);

    public async Task AddRoleAsync(
        Role role,
        CancellationToken cancellationToken)
    {
        await dbContext.Roles.AddAsync(role, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserRoleNamesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddUserRoleAsync(
        UserRole userRole,
        CancellationToken cancellationToken)
    {
        await dbContext.UserRoles.AddAsync(userRole, cancellationToken);
    }

    public Task<UserRole?> GetUserRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken)
        => dbContext.UserRoles
            .Include(ur => ur.Role)
            .SingleOrDefaultAsync(
                ur => ur.UserId == userId &&
                      ur.Role.Name == roleName,
                cancellationToken);

    public Task RemoveUserRoleAsync(
        UserRole userRole,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dbContext.UserRoles.Remove(userRole);

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<RoleWithCountDto>> GetAllRoles(
        CancellationToken cancellationToken)
    {
        return await dbContext.Roles
            .AsNoTracking()
            .Select(r => new RoleWithCountDto(
                r.Id,
                r.Name,
                r.Description,
                r.UserRoles.Count()))
            .ToListAsync(cancellationToken);
    }
}
