
namespace StockPilot.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponse> CreateCategory(
        Guid businessId,
        CategoryRequest request,
        CancellationToken cancellationToken);

    Task<CategoryResponse> GetCategoryById(
        Guid businessId,
        Guid id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryResponse>> GetAllCategory(
        Guid businessId,
        CancellationToken cancellationToken);

    Task<CategoryResponse> UpdateCategory(
        Guid businessId,
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken);

    Task DeleteCategory(
        Guid businessId,
        Guid id,
        CancellationToken cancellationToken);
}
