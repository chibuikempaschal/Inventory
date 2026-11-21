namespace StockPilot.Application.Interfaces
{
    public interface IUser
    {
        Task<AuthProfile?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserResponse?> GetUserByNameAsync(string name, CancellationToken cancellationToken);
        Task<UserResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<List<GetAllUserResponse>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<UserResponse?> UpdateUserAsync(Guid userId, UpdateUser updateUser, CancellationToken cancellationToken);
    }
}