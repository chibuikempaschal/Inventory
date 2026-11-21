
namespace StockPilot.Application.Interfaces.Repositories
{
    public interface ISaleItemRepo
    {
        // Add a sale item
        Task<SaleItem> AddAsync(
            SaleItem saleItem,
            CancellationToken cancellationToken);

        // Get a sale item by its ID
        Task<SaleItem?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken);

        // Get all items belonging to a particular sale
        Task<IReadOnlyList<SaleItem>> GetBySaleIdAsync(
            Guid saleId,
            CancellationToken cancellationToken);

        // Get all sale items
        Task<IReadOnlyList<SaleItem>> GetAllAsync(
            CancellationToken cancellationToken);

        // Update a sale item
        Task UpdateAsync(
            SaleItem saleItem,
            CancellationToken cancellationToken);

        // Delete a sale item
        Task DeleteAsync(
            SaleItem saleItem,
            CancellationToken cancellationToken);

        // Save database changes
        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}