// using StockPilot.Application.DTOs;
// using StockPilot.Domain.Entities;
// using StockPilot.Infrastructure.Persistence;
// using StockPilot.Domain.Entities;
// using StockPilot.Application.Interfaces.Repositories;
// using Microsoft.EntityFrameworkCore;

// namespace StockPilot.Infrastructure.Persistence.Repositories
// {
//     public class ProductRepo(InventoryDbContext dbContext) : IProductRepo
//     {
//         public async Task<Product> AddAsync(Product product, CancellationToken ct)
//         {
//             dbContext.Products.Add(product);
//             await dbContext.SaveChangesAsync(ct);
//             return product;
//         }

//         public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct)
//         {
//             return await dbContext.Products.AnyAsync(u => u.Name.ToLower() == name.ToLower(), ct);
//         }

//         public async Task<IReadOnlyList<ProductResponse>> GetAllProductsWithinCategoryAsync(string categoryName, CancellationToken ct)
//         {
//             return await dbContext.Products
//                 .Where(p => p.Category.Name.ToLower() == categoryName.ToLower())
//                 .Select(p => new ProductResponse(
//                      Id: p.Id,
//                      Sku: p.SKU,
//                      Name: p.Name,
//                      Description: p.Description,
//                      CategoryId: p.CategoryId,
//                      SellingPrice: p.SellingPrice,
//                      ImageUrl: p.ImageUrl,
//                      IsActive: p.IsActive,
//                      CreatedAt: p.CreatedAt
//                     ))
//                 .ToListAsync(ct);
//         }

//         public async Task<IEnumerable<Product>> GetByAllAsync(CancellationToken ct)
//         {
//             return await dbContext.Products
//             .Where(p => p.IsActive)
//             .AsNoTracking()
//         .ToListAsync(ct);
//         }

//         public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
//         {
//             return await dbContext.Products.FirstOrDefaultAsync(u => u.Id == id, ct);
//         }

//         public async Task<Product?> GetByNameAsync(string name, CancellationToken ct)
//         {
//             return await dbContext.Products.FirstOrDefaultAsync(u => u.Name == name, ct);
//         }

//         public Task SaveChangesAsync(CancellationToken ct)
//         {
//             return dbContext.SaveChangesAsync(ct);
//         }
//     }
// }


using Microsoft.EntityFrameworkCore;

namespace StockPilot.Infrastructure.Persistence.Repositories;

public class ProductRepo(InventoryDbContext dbContext) : IProductRepo
{
    public async Task<Product> AddAsync(
        Product product,
        CancellationToken ct)
    {
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync(ct);

        return product;
    }

    public async Task<bool> ExistsByNameAsync(
        Guid businessId,
        string name,
        CancellationToken ct)
    {
        return await dbContext.Products
            .AnyAsync(
                p => p.BusinessId == businessId &&
                     EF.Functions.ILike(p.Name, name),
                ct
            );
    }

    public async Task<IReadOnlyList<ProductResponse>>
        GetAllProductsWithinCategoryAsync(
            Guid businessId,
            string categoryName,
            CancellationToken ct)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p =>
                p.BusinessId == businessId &&
                p.Category.BusinessId == businessId &&
                EF.Functions.ILike(p.Category.Name, categoryName) &&
                p.IsActive
            )
            .Select(p => new ProductResponse(
                Id: p.Id,
                Sku: p.SKU,
                Name: p.Name,
                Description: p.Description,
                CategoryId: p.CategoryId,
                SellingPrice: p.SellingPrice,
                ImageUrl: p.ImageUrl,
                IsActive: p.IsActive,
                CreatedAt: p.CreatedAt
            ))
            .ToListAsync(ct);
    }
    public async Task<IEnumerable<Product>> GetByAllAsync(
        Guid businessId,
        CancellationToken ct)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p =>
                p.BusinessId == businessId &&
                p.IsActive
            )
            .ToListAsync(ct);
    }
    public async Task<Product?> GetByIdAsync(
        Guid businessId,
        Guid id,
        CancellationToken ct)
    {
        return await dbContext.Products
            .FirstOrDefaultAsync(
                p => p.Id == id &&
                     p.BusinessId == businessId,
                ct
            );
    }

    public async Task<Product?> GetByNameAsync(
        Guid businessId,
        string name,
        CancellationToken ct)
    {
        return await dbContext.Products
            .FirstOrDefaultAsync(
                p => p.BusinessId == businessId &&
                     EF.Functions.ILike(p.Name, name),
                ct
            );
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}
