
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StockPilot.Application.Interfaces;
using StockPilot.Domain.Entities;
using StockPilot.Infrastructure.Persistence;

namespace StockPilot.Infrastructure.Services;

public class DbAdminInitializer(
    InventoryDbContext dbContext,
    ILogger<DbAdminInitializer> logger) : IDbAdminInitializer
{
    public async Task SeedAsync(
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var adminEmail = configuration["SeedAdmin:Email"]?.Trim();

        if (string.IsNullOrWhiteSpace(adminEmail))
            throw new InvalidOperationException(
                "SeedAdmin:Email is not configured.");

        adminEmail = adminEmail.ToLowerInvariant();

        // Seed all platform and business roles.
        var roleDefinitions = new Dictionary<string, string>
        {
            ["SystemAdmin"] = "Manages the StockPilot platform.",
            ["Owner"] = "Owns and manages a business.",
            ["Manager"] = "Manages daily business operations.",
            ["Cashier"] = "Handles sales and payments.",
            ["Staff"] = "Performs assigned business tasks."
        };

        var existingRoles = await dbContext.Roles
            .ToListAsync(cancellationToken);

        foreach (var definition in roleDefinitions)
        {
            if (!existingRoles.Any(r =>
                r.Name.Equals(
                    definition.Key,
                    StringComparison.OrdinalIgnoreCase)))
            {
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = definition.Key,
                    Description = definition.Value
                };

                dbContext.Roles.Add(role);
                existingRoles.Add(role);

                logger.LogInformation(
                    "Seeded role {Role}.", definition.Key);
            }
        }

        // Find the existing user. Do not create a duplicate.
        var adminUser = await dbContext.Users
            .SingleOrDefaultAsync(
                u => u.Email.ToLower() == adminEmail,
                cancellationToken);

        if (adminUser is null)
            throw new InvalidOperationException(
                $"User '{adminEmail}' does not exist. Register this account first.");

        // Find SystemAdmin role.
        var systemAdminRole = existingRoles.Single(
            r => r.Name.Equals(
                "SystemAdmin",
                StringComparison.OrdinalIgnoreCase));

        // Assign SystemAdmin if not already assigned.
        var alreadyAssigned = await dbContext.UserRoles
            .AnyAsync(
                ur => ur.UserId == adminUser.Id &&
                      ur.RoleId == systemAdminRole.Id,
                cancellationToken);

        if (!alreadyAssigned)
        {
            dbContext.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = systemAdminRole.Id
            });

            logger.LogInformation(
                "Assigned SystemAdmin to {Email}.", adminEmail);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Database initialization completed.");
    }
}
