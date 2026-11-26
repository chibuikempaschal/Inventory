namespace StockPilot.Application.Interfaces.Services;

public interface IBusinessMemberService
{
    // Get all members belonging to a business.
    Task<IReadOnlyList<BusinessMemberResponse>> GetMembersAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken);

    // Add an existing StockPilot user to a business.
    Task<BusinessMemberResponse> AddMemberAsync(
        Guid userId,
        Guid businessId,
        BusinessMemberRequest request,
        CancellationToken cancellationToken);

    // Change the role of an existing business member.
    Task<BusinessMemberResponse> UpdateMemberRoleAsync(
        Guid userId,
        Guid businessId,
        Guid targetUserId,
        UpdateBusinessMemberRoleRequest request,
        CancellationToken cancellationToken);

    // Remove a member from a business.
    Task RemoveMemberAsync(
        Guid userId,
        Guid businessId,
        Guid targetUserId,
        CancellationToken cancellationToken);
}