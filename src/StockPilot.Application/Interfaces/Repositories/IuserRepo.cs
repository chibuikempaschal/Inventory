
namespace StockPilot.Application.Interfaces.Repositories
{
    public interface IuserRepo
    {
        // Returns the Entity, not DTOs
        Task<UserResponse?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserResponse?> GetByNameAsync(string name, CancellationToken cancellationToken);

        // Returns list of Entities with their roles included
        Task<List<GetAllUserResponse>> GetAllWithRolesAsync(CancellationToken cancellationToken);

        Task<AuthProfile?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserResponse?> UpdateProfileAsync(Guid userId, UpdateUser updateUser, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);

    }
}