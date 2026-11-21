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
    public class SaleItemRepo(InventoryDbContext dbContext) : ISaleItemRepo
    {
        public async Task<SaleItem> AddAsync(SaleItem saleItem, CancellationToken cancellationToken)
        {
            dbContext.SaleItems.Add(saleItem);
            await dbContext.SaveChangesAsync(cancellationToken);
            return saleItem;
        }

        public async Task DeleteAsync(SaleItem saleItem, CancellationToken cancellationToken)
        {
            dbContext.SaleItems.Remove(saleItem);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SaleItem>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await dbContext.SaleItems
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<SaleItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await dbContext.SaleItems.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<SaleItem>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken)
        {
            return await dbContext.SaleItems
                .AsNoTracking()
                .Where(s => s.SaleId == saleId)
                .ToListAsync(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(SaleItem saleItem, CancellationToken cancellationToken)
        {
            dbContext.SaleItems.Update(saleItem);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}