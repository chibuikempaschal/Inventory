
using StockPilot.Application.DTOs;
using StockPilot.Application.Interfaces;
using StockPilot.Application.Interfaces.Repositories;
using StockPilot.Application.Interfaces.Services;
using static StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Application.Features.Users;

public sealed class UserService(
    IuserRepo userRepo,
    IApplicationCache cache) : IUser
{
    private static class CacheTags
    {
        public const string Users = "users";
    }

    private static class CacheKeys
    {
        public const string AllUsers = "Users:All";

        public static string Profile(Guid userId)
            => $"Users:Profile:{userId}";

        public static string ById(Guid userId)
            => $"Users:Id:{userId}";

        public static string ByName(string name)
            => $"Users:Name:{name.ToLowerInvariant()}";
    }

    public async Task<List<GetAllUserResponse>> GetAllUsersAsync(
        CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(
            CacheKeys.AllUsers,
            async cancel =>
                await userRepo.GetAllWithRolesAsync(cancel),
            tags: [CacheTags.Users],
            cancellationToken: cancellationToken);
    }

    public async Task<AuthProfile?> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.", nameof(userId));

        return await cache.GetOrCreateAsync<AuthProfile?>(
            CacheKeys.Profile(userId),
            async cancel =>
                await userRepo.GetUserProfileAsync(userId, cancel),
            tags: [CacheTags.Users],
            cancellationToken: cancellationToken);
    }

    public async Task<UserResponse?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.", nameof(userId));

        return await cache.GetOrCreateAsync(
            CacheKeys.ById(userId),
            async cancel =>
                await userRepo.GetByIdAsync(userId, cancel)
                ?? throw new NotFoundException(
                    $"User with ID {userId} not found."),
            tags: [CacheTags.Users],
            cancellationToken: cancellationToken);
    }

    public async Task<UserResponse?> GetUserByNameAsync(
        string name,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Name cannot be empty.", nameof(name));

        var normalizedName = name.Trim();

        return await cache.GetOrCreateAsync(
            CacheKeys.ByName(normalizedName),
            async cancel =>
                await userRepo.GetByNameAsync(normalizedName, cancel)
                ?? throw new NotFoundException(
                    $"No user found with name '{normalizedName}'."),
            tags: [CacheTags.Users],
            cancellationToken: cancellationToken);
    }

    public async Task<UserResponse?> UpdateUserAsync(
        Guid userId,
        UpdateUser updateUser,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.", nameof(userId));

        ArgumentNullException.ThrowIfNull(updateUser);

        var user = await userRepo.UpdateProfileAsync(
            userId, updateUser, cancellationToken);

        if (user is null)
            throw new NotFoundException(
                $"User with ID {userId} not found.");

        await cache.RemoveByTagAsync(
            CacheTags.Users,
            cancellationToken);

        return user;
    }
}
