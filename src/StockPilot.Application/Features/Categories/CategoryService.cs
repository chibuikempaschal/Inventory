
using StockPilot.Application.Interfaces;
using StockPilot.Application.Interfaces.Repositories;
using StockPilot.Application.Interfaces.Services;
using static StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Application.Features.Categories;

public sealed class CategoryService(
    ICategoryRepo categoryRepo,
    IApplicationCache cache) : ICategoryService
{
    private static class CacheTags
    {
        public const string Categories = "categories";

        public static string BusinessCategories(Guid businessId)
            => $"categories:business:{businessId}";
    }

    private static class CacheKeys
    {
        public static string AllCategories(Guid businessId)
            => $"Categories:Business:{businessId}:All";

        public static string CategoryById(Guid businessId, Guid id)
            => $"Categories:Business:{businessId}:Id:{id}";
    }

    // ---------------------------------------------------------
    // CREATE CATEGORY
    // ---------------------------------------------------------

    public async Task<CategoryResponse> CreateCategory(
        Guid businessId,
        CategoryRequest request,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        ArgumentNullException.ThrowIfNull(request);

        var name = NormalizeName(request.Name);

        ValidateName(name);

        var description = NormalizeDescription(request.Description);

        if (await categoryRepo.ExistsByNameAsync(
            businessId,
            name,
            cancellationToken))
        {
            throw new AlreadyExistException(
                $"Category '{name}' already exists.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = name,
            Description = description,
            IsActive = true
        };

        await categoryRepo.AddAsync(
            category,
            cancellationToken);

        await InvalidateCategoryCacheAsync(
            businessId,
            cancellationToken);

        return MapToResponse(category);
    }

    // ---------------------------------------------------------
    // GET CATEGORY BY ID
    // ---------------------------------------------------------

    public async Task<CategoryResponse> GetCategoryById(
        Guid businessId,
        Guid id,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);
        ValidateCategoryId(id);

        return await cache.GetOrCreateAsync(
            CacheKeys.CategoryById(businessId, id),

            async cancel =>
            {
                var category = await categoryRepo.GetByIdAsync(
                    businessId,
                    id,
                    cancel);

                if (category is null)
                {
                    throw new NotFoundException(
                        $"Category with ID '{id}' was not found.");
                }

                return MapToResponse(category);
            },

            tags:
            [
                CacheTags.Categories,
                CacheTags.BusinessCategories(businessId)
            ],

            cancellationToken: cancellationToken);
    }

    // ---------------------------------------------------------
    // GET ALL CATEGORIES
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<CategoryResponse>> GetAllCategory(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        return await cache.GetOrCreateAsync(
            CacheKeys.AllCategories(businessId),

            async cancel =>
            {
                var categories = await categoryRepo.GetByAllAsync(
                    businessId,
                    cancel);

                return categories
                    .Select(MapToResponse)
                    .ToList();
            },

            tags:
            [
                CacheTags.Categories,
                CacheTags.BusinessCategories(businessId)
            ],

            cancellationToken: cancellationToken);
    }

    // ---------------------------------------------------------
    // UPDATE CATEGORY
    // ---------------------------------------------------------

    public async Task<CategoryResponse> UpdateCategory(
        Guid businessId,
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);
        ValidateCategoryId(id);

        ArgumentNullException.ThrowIfNull(request);

        if (request.Name is null &&
            request.Description is null)
        {
            throw new ArgumentException(
                "At least one category field must be provided.");
        }

        var category = await categoryRepo.GetByIdAsync(
            businessId,
            id,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Category with ID '{id}' was not found.");

        if (request.Name is not null)
        {
            var newName = NormalizeName(request.Name);

            ValidateName(newName);

            if (!string.Equals(
                category.Name,
                newName,
                StringComparison.OrdinalIgnoreCase))
            {
                if (await categoryRepo.ExistsByNameAsync(
                    businessId,
                    newName,
                    cancellationToken))
                {
                    throw new AlreadyExistException(
                        $"Category '{newName}' already exists.");
                }

                category.Name = newName;
            }
        }

        if (request.Description is not null)
        {
            category.Description =
                NormalizeDescription(request.Description);
        }

        await categoryRepo.SaveChangesAsync(
            cancellationToken);

        await InvalidateCategoryCacheAsync(
            businessId,
            cancellationToken);

        return MapToResponse(category);
    }

    // ---------------------------------------------------------
    // DELETE CATEGORY (SOFT DELETE)
    // ---------------------------------------------------------

    public async Task DeleteCategory(
        Guid businessId,
        Guid id,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);
        ValidateCategoryId(id);

        var category = await categoryRepo.GetByIdAsync(
            businessId,
            id,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Category with ID '{id}' was not found.");

        // Preserve product relationships and historical records.
        category.IsActive = false;

        await categoryRepo.SaveChangesAsync(
            cancellationToken);

        await InvalidateCategoryCacheAsync(
            businessId,
            cancellationToken);
    }

    // ---------------------------------------------------------
    // PRIVATE HELPERS
    // ---------------------------------------------------------

    private static void ValidateBusinessId(Guid businessId)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException(
                "Business ID is required.",
                nameof(businessId));
        }
    }

    private static void ValidateCategoryId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Category ID is required.",
                nameof(id));
        }
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Category name is required.");
        }

        return name.Trim();
    }

    private static void ValidateName(string name)
    {
        if (name.Length > 50)
        {
            throw new ArgumentException(
                "Category name cannot exceed 50 characters.");
        }
    }

    private static string? NormalizeDescription(string? description)
    {
        if (description is null)
            return null;

        var normalized = description.Trim();

        if (normalized.Length > 500)
        {
            throw new ArgumentException(
                "Category description cannot exceed 500 characters.");
        }

        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized;
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse(
            Id: category.Id,
            Name: category.Name,
            Description: category.Description,
            IsActive: category.IsActive);
    }

    private Task InvalidateCategoryCacheAsync(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        return cache.RemoveByTagAsync(
            CacheTags.BusinessCategories(businessId),
            cancellationToken);
    }
}
