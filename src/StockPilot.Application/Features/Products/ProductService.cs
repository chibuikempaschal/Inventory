using StockPilot.Application.Interfaces;
using StockPilot.Application.Interfaces.Repositories;
using StockPilot.Application.Interfaces.Services;
using static StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Application.Features.Products;

public class ProductService(
    IProductRepo productRepo,
    ICategoryRepo categoryRepo,
    IApplicationCache cache) : IProductService
{
    private static class CacheTags
    {
        public const string AllProducts = "products";

        public static string ById(Guid id) => $"product:{id}";
    }

    // Every cache key is scoped to a business.
    private static class CacheKeys
    {
        public static string AllProducts(Guid businessId)
            => $"Products:{businessId}:All";

        public static string ProductsAsAdmin(Guid businessId)
            => $"Products:{businessId}:Admin";

        public static string LowStockProducts(Guid businessId)
            => $"Products:{businessId}:LowStock";

        public static string ProductById(Guid businessId, Guid id)
            => $"Products:{businessId}:Id:{id}";

        public static string ProductByName(Guid businessId, string name)
            => $"Products:{businessId}:Name:{name.ToLowerInvariant()}";

        public static string ProductsWithinCategory(
            Guid businessId,
            string categoryName)
            => $"Products:{businessId}:Category:{categoryName.ToLowerInvariant()}";
    }

    // ---------------------------------------------------------
    // CREATE PRODUCT
    // ---------------------------------------------------------

    public async Task<CreateProductResponse?> CreateProductAsync(
        Guid businessId,
        CreateProductRequest createProductRequest,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        ArgumentNullException.ThrowIfNull(createProductRequest);

        if (string.IsNullOrWhiteSpace(createProductRequest.Sku))
            throw new ArgumentException("SKU is required.");

        if (string.IsNullOrWhiteSpace(createProductRequest.Name))
            throw new ArgumentException("Name is required.");

        if (string.IsNullOrWhiteSpace(createProductRequest.Description))
            throw new ArgumentException("Description is required.");

        if (string.IsNullOrWhiteSpace(createProductRequest.CategoryName))
            throw new ArgumentException("Category name is required.");

        if (createProductRequest.CostPrice <= 0)
            throw new ArgumentException("Cost price must be greater than zero.");

        if (createProductRequest.SellingPrice <= 0)
            throw new ArgumentException("Selling price must be greater than zero.");

        if (createProductRequest.SellingPrice < createProductRequest.CostPrice)
            throw new ArgumentException("Selling price cannot be lower than cost price.");

        if (createProductRequest.QuantityInStock < 0)
            throw new ArgumentException("Quantity in stock cannot be negative.");

        if (createProductRequest.ReorderLevel < 0)
            throw new ArgumentException("Reorder level cannot be negative.");

        var name = createProductRequest.Name.Trim();
        var sku = createProductRequest.Sku.Trim().ToUpperInvariant();

        if (await productRepo.ExistsByNameAsync(
            businessId, name, cancellationToken))
        {
            throw new AlreadyExistException(
                $"A product named '{name}' already exists.");
        }

        var category = await categoryRepo.GetByNameAsync(
            businessId,
            createProductRequest.CategoryName.Trim(),
            cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(
                $"Category '{createProductRequest.CategoryName}' does not exist.");
        }

        var product = new Product
        {
            BusinessId = businessId,
            SKU = sku,
            Name = name,
            Description = createProductRequest.Description.Trim(),
            CostPrice = createProductRequest.CostPrice,
            SellingPrice = createProductRequest.SellingPrice,
            CategoryId = category.Id,
            QuantityInStock = createProductRequest.QuantityInStock,
            ReorderLevel = createProductRequest.ReorderLevel,
            ImageUrl = createProductRequest.ImageUrl
        };

        await productRepo.AddAsync(product, cancellationToken);

        await InvalidateProductCache(cancellationToken);

        return new CreateProductResponse(
            product.Id,
            product.SKU,
            product.Name,
            product.Description,
            product.CategoryId,
            product.SellingPrice,
            product.QuantityInStock,
            product.ImageUrl,
            product.IsActive,
            product.CreatedAt
        );
    }

    // ---------------------------------------------------------
    // GET ALL PRODUCTS - ADMIN
    // ---------------------------------------------------------

    public async Task<IEnumerable<AdminProductResponse>>
        GetAllProductsAsAdminAsync(
            Guid businessId,
            CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        return await cache.GetOrCreateAsync(
            CacheKeys.ProductsAsAdmin(businessId),

            async cancel =>
            {
                var products = await productRepo.GetByAllAsync(
                    businessId, cancel);

                if (!products.Any())
                    throw new NotFoundException("No products found.");

                return products.Select(p => new AdminProductResponse(
                    Id: p.Id,
                    Sku: p.SKU,
                    Name: p.Name,
                    Description: p.Description,
                    CategoryId: p.CategoryId,
                    SellingPrice: p.SellingPrice,
                    QuantityInStock: p.QuantityInStock,
                    ReorderLevel: p.ReorderLevel,
                    StockStatus: GetStockStatus(
                        p.QuantityInStock, p.ReorderLevel),
                    ImageUrl: p.ImageUrl,
                    IsActive: p.IsActive,
                    CreatedAt: p.CreatedAt,
                    UpdatedAt: p.UpdatedAt
                )).ToList();
            },

            tags: [CacheTags.AllProducts],
            cancellationToken: cancellationToken
        );
    }

    // ---------------------------------------------------------
    // GET ALL PRODUCTS
    // ---------------------------------------------------------

    public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        return await cache.GetOrCreateAsync(
            CacheKeys.AllProducts(businessId),

            async cancel =>
            {
                var products = await productRepo.GetByAllAsync(
                    businessId, cancel);

                if (!products.Any())
                    throw new NotFoundException("No products found.");

                return products.Select(p => new ProductResponse(
                    Id: p.Id,
                    Sku: p.SKU,
                    Name: p.Name,
                    Description: p.Description,
                    CategoryId: p.CategoryId,
                    SellingPrice: p.SellingPrice,
                    ImageUrl: p.ImageUrl,
                    IsActive: p.IsActive,
                    CreatedAt: p.CreatedAt
                )).ToList();
            },

            tags: [CacheTags.AllProducts],
            cancellationToken: cancellationToken
        );
    }

    // ---------------------------------------------------------
    // GET PRODUCT BY ID
    // ---------------------------------------------------------

    public async Task<ProductResponse> GetProductByIdAsync(
        Guid businessId,
        Guid id,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        if (id == Guid.Empty)
            throw new ArgumentException("Product ID is required.");

        return await cache.GetOrCreateAsync(
            CacheKeys.ProductById(businessId, id),

            async cancel =>
            {
                var product = await productRepo.GetByIdAsync(
                    businessId, id, cancel);

                if (product is null)
                    throw new NotFoundException(
                        $"Product with ID {id} was not found.");

                return MapProductResponse(product);
            },

            tags:
            [
                CacheTags.AllProducts,
                CacheTags.ById(id)
            ],

            cancellationToken: cancellationToken
        );
    }

    // ---------------------------------------------------------
    // GET PRODUCT BY NAME
    // ---------------------------------------------------------

    public async Task<ProductResponse> GetProductByNameAsync(
        Guid businessId,
        string name,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.");

        var normalizedName = name.Trim();

        return await cache.GetOrCreateAsync(
            CacheKeys.ProductByName(businessId, normalizedName),

            async cancel =>
            {
                var product = await productRepo.GetByNameAsync(
                    businessId, normalizedName, cancel);

                if (product is null)
                    throw new NotFoundException(
                        $"Product '{normalizedName}' was not found.");

                return MapProductResponse(product);
            },

            tags: [CacheTags.AllProducts],
            cancellationToken: cancellationToken
        );
    }

    // ---------------------------------------------------------
    // GET LOW STOCK PRODUCTS
    // ---------------------------------------------------------

    public async Task<IEnumerable<LowStockProductResponse>>
        GetLowStockProductsAsync(
            Guid businessId,
            CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        return await cache.GetOrCreateAsync(
            CacheKeys.LowStockProducts(businessId),

            async cancel =>
            {
                var products = await productRepo.GetByAllAsync(
                    businessId, cancel);

                if (!products.Any())
                    throw new NotFoundException("No products found.");

                return products
                    .Where(p => p.QuantityInStock <= p.ReorderLevel)
                    .Select(p => new LowStockProductResponse(
                        Sku: p.SKU,
                        Name: p.Name,
                        Description: p.Description,
                        QuantityInStock: p.QuantityInStock,
                        ReorderLevel: p.ReorderLevel,
                        StockStatus: p.QuantityInStock <= 0
                            ? StockStatus.OutOfStock
                            : StockStatus.LowStock,
                        ImageUrl: p.ImageUrl,
                        CreatedAt: p.CreatedAt
                    ))
                    .ToList();
            },

            tags: [CacheTags.AllProducts],
            cancellationToken: cancellationToken
        );
    }

    // ---------------------------------------------------------
    // UPDATE PRODUCT
    // ---------------------------------------------------------

    public async Task<AdminProductResponse> UpdateProductAsync(
        Guid businessId,
        Guid id,
        UpdateProductRequest updateProductRequest,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        ArgumentNullException.ThrowIfNull(updateProductRequest);

        if (id == Guid.Empty)
            throw new ArgumentException("Product ID is required.");

        var product = await productRepo.GetByIdAsync(
            businessId, id, cancellationToken);

        if (product is null)
            throw new NotFoundException(
                $"Product with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(updateProductRequest.Sku))
        {
            product.SKU = updateProductRequest.Sku
                .Trim()
                .ToUpperInvariant();
        }
        else
        {
            throw new ArgumentException("SKU is required.");
        }

        if (!string.IsNullOrWhiteSpace(updateProductRequest.Name))
        {
            var newName = updateProductRequest.Name.Trim();

            if (!string.Equals(
                product.Name,
                newName,
                StringComparison.OrdinalIgnoreCase))
            {
                if (await productRepo.ExistsByNameAsync(
                    businessId, newName, cancellationToken))
                {
                    throw new AlreadyExistException(
                        $"A product named '{newName}' already exists.");
                }
            }

            product.Name = newName;
        }

        if (!string.IsNullOrWhiteSpace(updateProductRequest.Description))
            product.Description = updateProductRequest.Description.Trim();

        if (!string.IsNullOrWhiteSpace(updateProductRequest.CategoryName))
        {
            var category = await categoryRepo.GetByNameAsync(
                businessId,
                updateProductRequest.CategoryName.Trim(),
                cancellationToken);

            if (category is null)
                throw new NotFoundException(
                    $"Category '{updateProductRequest.CategoryName}' was not found.");

            product.CategoryId = category.Id;
        }

        if (updateProductRequest.CostPrice is decimal costPrice)
        {
            if (costPrice <= 0)
                throw new ArgumentException("Cost price must be greater than zero.");

            product.CostPrice = costPrice;
        }

        if (updateProductRequest.SellingPrice is decimal sellingPrice)
        {
            if (sellingPrice <= 0)
                throw new ArgumentException("Selling price must be greater than zero.");

            product.SellingPrice = sellingPrice;
        }

        if (updateProductRequest.QuantityInStock is int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative.");

            product.QuantityInStock = quantity;
        }

        if (updateProductRequest.ReorderLevel is int reorderLevel)
        {
            if (reorderLevel < 0)
                throw new ArgumentException("Reorder level cannot be negative.");

            product.ReorderLevel = reorderLevel;
        }

        if (product.SellingPrice < product.CostPrice)
            throw new ArgumentException(
                "Selling price cannot be lower than cost price.");

        product.UpdatedAt = DateTime.UtcNow;

        await productRepo.SaveChangesAsync(cancellationToken);

        await InvalidateProductCache(cancellationToken);

        return new AdminProductResponse(
            Id: product.Id,
            Sku: product.SKU,
            Name: product.Name,
            Description: product.Description,
            CategoryId: product.CategoryId,
            SellingPrice: product.SellingPrice,
            QuantityInStock: product.QuantityInStock,
            ReorderLevel: product.ReorderLevel,
            StockStatus: GetStockStatus(
                product.QuantityInStock, product.ReorderLevel),
            ImageUrl: product.ImageUrl,
            IsActive: product.IsActive,
            CreatedAt: product.CreatedAt,
            UpdatedAt: product.UpdatedAt
        );
    }

    // ---------------------------------------------------------
    // GET PRODUCTS WITHIN CATEGORY
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<ProductResponse>>
        GettingAllProductsWithinCategoryAsync(
            Guid businessId,
            string categoryName,
            CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        if (string.IsNullOrWhiteSpace(categoryName))
            throw new ArgumentException("Category name is required.");

        var normalizedCategoryName = categoryName.Trim();

        return await cache.GetOrCreateAsync(
            CacheKeys.ProductsWithinCategory(
                businessId, normalizedCategoryName),

            async cancel =>
            {
                var category = await categoryRepo.GetByNameAsync(
                    businessId,
                    normalizedCategoryName,
                    cancel);

                if (category is null)
                    throw new NotFoundException(
                        $"Category '{normalizedCategoryName}' was not found.");

                return await productRepo.GetAllProductsWithinCategoryAsync(
                    businessId,
                    category.Name,
                    cancel);
            },

            tags: [CacheTags.AllProducts],
            cancellationToken: cancellationToken
        );
    }

    // ---------------------------------------------------------
    // DELETE PRODUCT (SOFT DELETE)
    // ---------------------------------------------------------

    public async Task DeleteProductAsync(
        Guid businessId,
        Guid id,
        CancellationToken cancellationToken)
    {
        ValidateBusinessId(businessId);

        if (id == Guid.Empty)
            throw new ArgumentException("Product ID is required.");

        var product = await productRepo.GetByIdAsync(
            businessId, id, cancellationToken);

        if (product is null)
            throw new NotFoundException(
                $"Product with ID {id} was not found.");

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await productRepo.SaveChangesAsync(cancellationToken);

        await InvalidateProductCache(cancellationToken);
    }

    // ---------------------------------------------------------
    // PRIVATE HELPERS
    // ---------------------------------------------------------

    private static void ValidateBusinessId(Guid businessId)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("Business ID is required.");
    }

    private static StockStatus GetStockStatus(
        int quantityInStock,
        int reorderLevel)
    {
        return quantityInStock <= 0
            ? StockStatus.OutOfStock
            : quantityInStock <= reorderLevel
                ? StockStatus.LowStock
                : StockStatus.InStock;
    }

    private static ProductResponse MapProductResponse(Product product)
    {
        return new ProductResponse(
            Id: product.Id,
            Sku: product.SKU,
            Name: product.Name,
            Description: product.Description,
            CategoryId: product.CategoryId,
            SellingPrice: product.SellingPrice,
            ImageUrl: product.ImageUrl,
            IsActive: product.IsActive,
            CreatedAt: product.CreatedAt
        );
    }

    private Task InvalidateProductCache(CancellationToken cancellationToken)
    {
        return cache.RemoveByTagAsync(
            CacheTags.AllProducts,
            cancellationToken);
    }
}
