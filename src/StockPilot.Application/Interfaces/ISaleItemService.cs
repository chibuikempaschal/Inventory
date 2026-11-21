namespace StockPilot.Application.Interfaces
{
    public interface ISaleItemService
    {

        Task<SaleItemResponse> CreateSaleItemAsync(
            Guid saleId,
            CreateSaleItemRequest request,
            CancellationToken cancellationToken);

        Task<SaleItemResponse> GetSaleItemByIdAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<SaleItemResponse>> GetSaleItemsBySaleIdAsync(
            Guid saleId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<AdminSaleItemResponse>> GetAllSaleItemsAsync(
            CancellationToken cancellationToken);

        Task DeleteSaleItemAsync(
            Guid id,
            CancellationToken cancellationToken);
    }

}