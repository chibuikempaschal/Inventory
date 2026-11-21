
namespace StockPilot.Application.Interfaces;

public interface IBusinessService
{
    Task<BusinessResponse> CreateBusinessAsync(
        Guid userId,
        CreateBusinessRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MyBusinessResponse>> GetMyBusinessesAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<BusinessResponse> GetBusinessByIdAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken);

    Task<BusinessResponse> UpdateBusinessAsync(
        Guid userId,
        Guid businessId,
        UpdateBusinessRequest request,
        CancellationToken cancellationToken);

    Task DeactivateBusinessAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken);
}
