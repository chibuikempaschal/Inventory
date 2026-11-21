
using StockPilot.Domain.Entities;

namespace StockPilot.Application.Interfaces.Repositories;

public interface IBusinessRepo
{
    Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken);

    Task AddBusinessWithOwnerAsync(
        Business business,
        BusinessUser ownerMembership,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<BusinessUser>> GetUserBusinessesAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<BusinessUser?> GetMembershipAsync(
        Guid businessId,
        Guid userId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}
