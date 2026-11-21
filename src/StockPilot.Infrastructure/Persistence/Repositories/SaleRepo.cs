using StockPilot.Application.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPilot.Infrastructure.Persistence;
using StockPilot.Domain.Entities;
using StockPilot.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace StockPilot.Infrastructure.Persistence.Repositories
{
    public class SaleRepo(InventoryDbContext context) : ISaleRepo
    {
        public async Task<Sale> AddSaleAsync(Sale sale, CancellationToken ct)
        {
            context.Sales.Add(sale);
            await context.SaveChangesAsync(ct);
            return sale;
        }

        public async Task<List<Product>> GetProductsForSaleAsync(
     IReadOnlyCollection<Guid> productIds,
     CancellationToken ct)
        {
            // Tracked query — we will mutate these (stock) and SaveChangesAsync will persist.
            return await context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(ct);
        }

        public async Task<string> GenerateReceiptNumberAsync(CancellationToken ct)
        {
            // Cheap, collision-resistant, human-readable: RCP-YYYYMMDD-XXXX
            // In a real system you'd use a DB sequence or a unique index + retry.
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await context.Sales
                .CountAsync(s => s.SaleDate.Date == DateTime.UtcNow.Date, ct);
            return $"RCP-{datePart}-{(count + 1):D4}";
        }

        public async Task<Sale?> GetSaleByIdAsync(Guid id, CancellationToken ct)
        {
            return await context.Sales
                .Include(s => s.SaleItems)
                .Include(s => s.Payments)
                .Include(s => s.StockMovements)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<IEnumerable<Sale>> ListSalesAsync(CancellationToken ct)
        {
            return await context.Sales
                .Include(s => s.SaleItems)
                .Include(s => s.Payments)
                .Include(s => s.StockMovements)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Sale>> ListSalesByStatusAsync(SaleStatus status, CancellationToken ct)
        {
            return await context.Sales.AsNoTracking()
                                 .Where(s => s.Status == status)
                                 .OrderByDescending(s => s.SaleDate)
                                 .ToListAsync(ct);
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return context.SaveChangesAsync(ct);
        }
    }
}