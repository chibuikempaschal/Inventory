namespace StockPilot.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

        Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken);

        Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);

    }
}