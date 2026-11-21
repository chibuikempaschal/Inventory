using StockPilot.Application.Features.Roles;
using StockPilot.Application.Interfaces.Repositories;
using StockPilot.Domain.Entities;

namespace StockPilot.UnitTests;

public sealed class RoleServiceTests
{
    [Fact]
    public async Task AssignRoleAsync_CreatesRoleAndAssignment_WhenRoleDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var repository = new FakeRoleRepository(userId);
        var service = new RoleService(repository);

        var result = await service.AssignRoleAsync(userId, "Manager", "Can manage stock", CancellationToken.None);

        Assert.Equal("Manager", result.AssignedRole);
        Assert.Equal("Can manage stock", result.RoleDescription);
        Assert.Contains("Manager", result.CurrentRoles);
        Assert.Empty(result.PreviousRoles);
        Assert.Single(repository.Assignments);
    }

    [Fact]
    public async Task AssignRoleAsync_DoesNotCreateDuplicateAssignment()
    {
        var userId = Guid.NewGuid();
        var repository = new FakeRoleRepository(userId);
        var role = new Role { Name = "Manager" };
        repository.Roles.Add(role);
        repository.Assignments.Add(new UserRole { UserId = userId, RoleId = role.Id });
        var service = new RoleService(repository);

        var result = await service.AssignRoleAsync(userId, "Manager", null, CancellationToken.None);

        Assert.Single(repository.Assignments);
        Assert.Contains("Manager", result.CurrentRoles);
    }

    [Fact]
    public async Task GetRolesAsync_ReturnsOnlyRolesForRequestedUser()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var repository = new FakeRoleRepository(userId, otherUserId);
        var admin = new Role { Name = "Admin" };
        var cashier = new Role { Name = "Cashier" };
        repository.Roles.AddRange([admin, cashier]);
        repository.Assignments.Add(new UserRole { UserId = userId, RoleId = cashier.Id });
        repository.Assignments.Add(new UserRole { UserId = otherUserId, RoleId = admin.Id });
        var service = new RoleService(repository);

        var roles = await service.GetRolesAsync(userId, CancellationToken.None);

        Assert.Single(roles);
        Assert.Equal("Cashier", roles[0]);
    }

    private sealed class FakeRoleRepository(params Guid[] existingUsers) : IRoleRepository
    {
        private readonly HashSet<Guid> _users = existingUsers.ToHashSet();
        public List<Role> Roles { get; } = [];
        public List<UserRole> Assignments { get; } = [];

        public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
            => Task.FromResult(_users.Contains(userId));

        public Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
            => Task.FromResult(Roles.SingleOrDefault(r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase)));

        public Task AddRoleAsync(Role role, CancellationToken cancellationToken)
        {
            Roles.Add(role);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken)
        {
            IReadOnlyList<string> names = Assignments
                .Where(a => a.UserId == userId)
                .Join(Roles, a => a.RoleId, r => r.Id, (_, role) => role.Name)
                .ToList();
            return Task.FromResult(names);
        }

        public Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken)
        {
            Assignments.Add(userRole);
            return Task.CompletedTask;
        }

        public Task<UserRole?> GetUserRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
        {
            var assignment = Assignments.FirstOrDefault(a => a.UserId == userId &&
                Roles.Any(r => r.Id == a.RoleId && string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase)));
            return Task.FromResult(assignment);
        }

        public Task RemoveUserRoleAsync(UserRole userRole, CancellationToken cancellationToken)
        {
            Assignments.Remove(userRole);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
