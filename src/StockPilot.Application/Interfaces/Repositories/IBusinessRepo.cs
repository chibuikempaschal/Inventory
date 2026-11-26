
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

    //Business Member
    Task<BusinessUser?> GetMembershipAsync(
        Guid businessId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<BusinessUser>> GetBusinessMembersAsync(
    Guid businessId,
    CancellationToken cancellationToken);

    Task<BusinessUser?> GetBusinessMemberAsync(
        Guid businessId,
        Guid targetUserId,
        CancellationToken cancellationToken);

    Task<User?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<bool> MembershipExistsAsync(
        Guid businessId,
        Guid userId,
        CancellationToken cancellationToken);

    Task AddMembershipAsync(
        BusinessUser membership,
        CancellationToken cancellationToken);


    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}
