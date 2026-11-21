
namespace StockPilot.Application.Interfaces.Repositories
{
    public interface ISaleRepo
    {
        Task<Sale> AddSaleAsync(Sale sale, CancellationToken ct);
        Task<Sale?> GetSaleByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<Sale>> ListSalesAsync(CancellationToken ct);
        Task<IEnumerable<Sale>> ListSalesByStatusAsync(SaleStatus status, CancellationToken ct);
        Task<List<Product>> GetProductsForSaleAsync(IReadOnlyCollection<Guid> productIds, CancellationToken ct);
        Task<string> GenerateReceiptNumberAsync(CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}