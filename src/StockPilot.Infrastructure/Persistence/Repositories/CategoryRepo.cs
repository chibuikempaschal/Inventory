
using Microsoft.EntityFrameworkCore;

namespace StockPilot.Infrastructure.Persistence.Repositories;

public class CategoryRepo(InventoryDbContext dbContext) : ICategoryRepo
{
    public async Task<Category> AddAsync(
        Category category,
        CancellationToken cancellationToken)
    {
        await dbContext.Categories.AddAsync(
            category,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return category;
    }

    public async Task<bool> ExistsByNameAsync(
        Guid businessId,
        string name,
        CancellationToken cancellationToken)
    {
        if (businessId == Guid.Empty ||
            string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var normalizedName = name.Trim();

        return await dbContext.Categories.AnyAsync(
            category =>
                category.BusinessId == businessId &&
                EF.Functions.ILike(category.Name, normalizedName),
            cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetByAllAsync(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(category =>
                category.BusinessId == businessId &&
                category.IsActive)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(
        Guid businessId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .FirstOrDefaultAsync(
                category =>
                    category.Id == id &&
                    category.BusinessId == businessId &&
                    category.IsActive,
                cancellationToken);
    }

    public async Task<Category?> GetByNameAsync(
        Guid businessId,
        string name,
        CancellationToken cancellationToken)
    {
        if (businessId == Guid.Empty ||
            string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var normalizedName = name.Trim();

        return await dbContext.Categories
            .FirstOrDefaultAsync(
                category =>
                    category.BusinessId == businessId &&
                    category.IsActive &&
                    EF.Functions.ILike(
                        category.Name,
                        normalizedName),
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
