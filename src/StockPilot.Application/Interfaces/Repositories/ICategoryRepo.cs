
namespace StockPilot.Application.Interfaces.Repositories;

public interface ICategoryRepo
{
    Task<Category?> GetByIdAsync(
        Guid businessId,
        Guid id,
        CancellationToken cancellationToken);

    Task<Category?> GetByNameAsync(
        Guid businessId,
        string name,
        CancellationToken cancellationToken);

    Task<IEnumerable<Category>> GetByAllAsync(
        Guid businessId,
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(
        Guid businessId,
        string name,
        CancellationToken cancellationToken);

    Task<Category> AddAsync(
        Category category,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}
