
using Microsoft.EntityFrameworkCore;

namespace StockPilot.Infrastructure.Persistence.Repositories;

public sealed class BusinessRepo(
    InventoryDbContext dbContext) : IBusinessRepo
{
    public async Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        return await dbContext.Businesses
            .AnyAsync(
                business => business.Slug == slug,
                cancellationToken);
    }

    public async Task AddBusinessWithOwnerAsync(
        Business business,
        BusinessUser ownerMembership,
        CancellationToken cancellationToken)
    {
        await dbContext.Businesses.AddAsync(
            business,
            cancellationToken);

        await dbContext.BusinessUsers.AddAsync(
            ownerMembership,
            cancellationToken);

        // Both records are saved in one SaveChanges operation.
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BusinessUser>> GetUserBusinessesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.BusinessUsers
            .AsNoTracking()
            .Include(membership => membership.Business)
            .Include(membership => membership.Role)
            .Where(membership =>
                membership.UserId == userId &&
                membership.IsActive &&
                membership.Business.IsActive)
            .OrderBy(membership => membership.Business.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<BusinessUser?> GetMembershipAsync(
        Guid businessId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.BusinessUsers
            .Include(membership => membership.Business)
            .Include(membership => membership.Role)
            .FirstOrDefaultAsync(
                membership =>
                    membership.BusinessId == businessId &&
                    membership.UserId == userId &&
                    membership.IsActive &&
                    membership.Business.IsActive,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
