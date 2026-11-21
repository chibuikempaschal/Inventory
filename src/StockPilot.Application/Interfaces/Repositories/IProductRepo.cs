
namespace StockPilot.Application.Interfaces.Repositories
{
    public interface IProductRepo
    {
        // Task<ProductResponse?> CreatingProductAsync(CreateProductRequest createProductRequest, CancellationToken cancellationToken);

        Task<Product?> GetByNameAsync(Guid businessId, string name, CancellationToken ct);
        Task<IEnumerable<Product>> GetByAllAsync(Guid businessId, CancellationToken ct);
        Task<Product?> GetByIdAsync(Guid businessId, Guid id, CancellationToken ct);
        Task<bool> ExistsByNameAsync(Guid businessId, string name, CancellationToken ct);
        Task<IReadOnlyList<ProductResponse>> GetAllProductsWithinCategoryAsync(Guid businessId, string categoryName, CancellationToken ct);
        Task<Product> AddAsync(Product product, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}