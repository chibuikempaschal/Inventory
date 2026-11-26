
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

    // Gets active members of a business.
    // Used for displaying the member list.
    public async Task<IReadOnlyList<BusinessUser>> GetBusinessMembersAsync(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        return await dbContext.BusinessUsers
            .AsNoTracking()
            .Include(membership => membership.User)
            .Include(membership => membership.Role)
            .Where(membership =>
                membership.BusinessId == businessId &&
                membership.IsActive)
            .OrderBy(membership => membership.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    // Gets a membership regardless of whether it is active.
    // Important: inactive memberships can be reactivated.
    public async Task<BusinessUser?> GetBusinessMemberAsync(
        Guid businessId,
        Guid targetUserId,
        CancellationToken cancellationToken)
    {
        return await dbContext.BusinessUsers
            .Include(membership => membership.User)
            .Include(membership => membership.Role)
            .FirstOrDefaultAsync(
                membership =>
                    membership.BusinessId == businessId &&
                    membership.UserId == targetUserId,
                cancellationToken);
    }

    // Finds an existing StockPilot account using its email.
    public async Task<User?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await dbContext.Users
            .FirstOrDefaultAsync(
                user => user.Email.ToLower() == normalizedEmail,
                cancellationToken);
    }

    // Checks whether a membership record already exists,
    // including inactive memberships.
    public async Task<bool> MembershipExistsAsync(
        Guid businessId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.BusinessUsers
            .AnyAsync(
                membership =>
                    membership.BusinessId == businessId &&
                    membership.UserId == userId,
                cancellationToken);
    }

    public async Task AddMembershipAsync(
        BusinessUser membership,
        CancellationToken cancellationToken)
    {
        await dbContext.BusinessUsers.AddAsync(
            membership,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
