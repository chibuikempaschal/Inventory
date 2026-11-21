namespace StockPilot.Application.Interfaces
{
    public interface ISaleService
    {
        // Create a sale with its items
        Task<SaleDetailsResponse> CreateSaleAsync(CreateSaleRequest request, CancellationToken ct);

        // Get a sale by ID
        Task<SaleDetailsResponse> GetSaleByIdAsync(Guid id, CancellationToken ct);

        // Get a sale by receipt number
        Task<SaleDetailsResponse> GetSaleByReceiptNumberAsync(string receiptNumber, CancellationToken ct);

        // Get all sales
        Task<IReadOnlyList<SaleResponse>> GetAllSalesAsync(CancellationToken ct);

        // Get sales by status
        Task<IReadOnlyList<SaleResponse>> GetSalesByStatusAsync(SaleStatus status, CancellationToken ct);

        // Get sales processed by a particular user
        Task<IReadOnlyList<SaleResponse>> GetSalesByUserIdAsync(Guid userId, CancellationToken ct);

        // Update sale notes
        Task<SaleResponse> UpdateSaleAsync(Guid id, UpdateSaleRequest request, CancellationToken ct);

        // Cancel a sale
        Task<SaleResponse> CancelSaleAsync(Guid id, CancellationToken ct);
    }
}