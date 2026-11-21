
namespace StockPilot.Application.Interfaces;

public interface IProductService
{
    Task<CreateProductResponse?> CreateProductAsync(
        Guid businessId,
        CreateProductRequest createProductRequest,
        CancellationToken cancellationToken);

    Task<ProductResponse> GetProductByIdAsync(
        Guid businessId,
        Guid id,
        CancellationToken cancellationToken);

    Task<ProductResponse> GetProductByNameAsync(
        Guid businessId,
        string name,
        CancellationToken cancellationToken);

    Task<IEnumerable<ProductResponse>> GetAllProductsAsync(
        Guid businessId,
        CancellationToken cancellationToken);

    Task<IEnumerable<AdminProductResponse>> GetAllProductsAsAdminAsync(
        Guid businessId,
        CancellationToken cancellationToken);

    Task<IEnumerable<LowStockProductResponse>> GetLowStockProductsAsync(
        Guid businessId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductResponse>>
        GettingAllProductsWithinCategoryAsync(
            Guid businessId,
            string categoryName,
            CancellationToken cancellationToken);

    Task DeleteProductAsync(
        Guid businessId,
        Guid id,
        CancellationToken cancellationToken);

    Task<AdminProductResponse> UpdateProductAsync(
        Guid businessId,
        Guid id,
        UpdateProductRequest updateProductRequest,
        CancellationToken cancellationToken);
}
