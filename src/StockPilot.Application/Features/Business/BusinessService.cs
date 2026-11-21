
using System.Text.RegularExpressions;
using StockPilot.Application.DTOs;
using StockPilot.Application.Interfaces;
using StockPilot.Application.Interfaces.Repositories;
using StockPilot.Application.Interfaces.Services;
using StockPilot.Domain.Constants;
using StockPilot.Domain.Entities;
using static StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Application.Features.Businesses;

public sealed class BusinessService(
    IBusinessRepo businessRepo,
    IRoleRepository roleRepository,
    IApplicationCache cache) : IBusinessService
{
    // ---------------------------------------------------------
    // CACHE TAGS AND KEYS
    // ---------------------------------------------------------

    private static class CacheTags
    {
        public const string Businesses = "businesses";

        public static string UserBusinesses(Guid userId)
            => $"businesses:user:{userId}";
    }

    private static class CacheKeys
    {
        public static string UserBusinesses(Guid userId)
            => $"Businesses:User:{userId}:All";
    }

    // ---------------------------------------------------------
    // CREATE BUSINESS
    // ---------------------------------------------------------

    public async Task<BusinessResponse> CreateBusinessAsync(
        Guid userId,
        CreateBusinessRequest request,
        CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        ArgumentNullException.ThrowIfNull(request);

        var name = NormalizeRequired(
            request.Name,
            "Business name");

        if (name.Length > 150)
        {
            throw new ArgumentException(
                "Business name cannot exceed 150 characters.");
        }

        // Ensure the user exists.
        if (!await roleRepository.UserExistsAsync(
            userId,
            cancellationToken))
        {
            throw new NotFoundException(
                $"User '{userId}' was not found.");
        }

        // The Owner role must already exist in the Roles table.
        var ownerRole = await roleRepository.GetRoleByNameAsync(
            BusinessRoles.Owner,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Owner role has not been seeded. Run database initialization first.");

        var businessId = Guid.NewGuid();

        var slug = await GenerateUniqueSlugAsync(
            name,
            cancellationToken);

        var business = new Business
        {
            Id = businessId,
            Name = name,
            Slug = slug,
            Description = NormalizeOptional(request.Description),
            Email = NormalizeOptional(request.Email)?.ToLowerInvariant(),
            PhoneNumber = NormalizeOptional(request.PhoneNumber),
            Address = NormalizeOptional(request.Address),
            City = NormalizeOptional(request.City),
            State = NormalizeOptional(request.State),
            CountryCode = NormalizeCode(
                request.CountryCode,
                "NG",
                2),
            CurrencyCode = NormalizeCode(
                request.CurrencyCode,
                "NGN",
                3),
            TimeZoneId = NormalizeOptional(request.TimeZoneId),
            LogoUrl = NormalizeOptional(request.LogoUrl),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var ownerMembership = new BusinessUser
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            UserId = userId,
            RoleId = ownerRole.Id,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Save the business and its owner membership together.
        await businessRepo.AddBusinessWithOwnerAsync(
            business,
            ownerMembership,
            cancellationToken);

        // The creator's cached business list is now stale.
        await cache.RemoveByTagAsync(
            CacheTags.UserBusinesses(userId),
            cancellationToken);

        return MapToResponse(
            business,
            BusinessRoles.Owner);
    }

    // ---------------------------------------------------------
    // GET MY BUSINESSES
    // ---------------------------------------------------------

    public async Task<IReadOnlyList<MyBusinessResponse>>
        GetMyBusinessesAsync(
            Guid userId,
            CancellationToken cancellationToken)
    {
        ValidateUserId(userId);

        return await cache.GetOrCreateAsync(
            CacheKeys.UserBusinesses(userId),

            async cancel =>
            {
                var memberships =
                    await businessRepo.GetUserBusinessesAsync(
                        userId,
                        cancel);

                return (IReadOnlyList<MyBusinessResponse>)
                    memberships
                        .Select(membership => new MyBusinessResponse(
                            BusinessId: membership.BusinessId,
                            Name: membership.Business.Name,
                            Slug: membership.Business.Slug,
                            CurrencyCode: membership.Business.CurrencyCode,
                            IsActive: membership.Business.IsActive,
                            YourRole: membership.Role.Name,
                            JoinedAt: membership.JoinedAt))
                        .ToList();
            },

            tags:
            [
                CacheTags.Businesses,
                CacheTags.UserBusinesses(userId)
            ],

            cancellationToken: cancellationToken);
    }

    // ---------------------------------------------------------
    // GET BUSINESS BY ID
    // ---------------------------------------------------------

    public async Task<BusinessResponse> GetBusinessByIdAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        // Always verify membership from the database.
        // Cached business data must never bypass authorization.
        var membership = await GetRequiredMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        return MapToResponse(
            membership.Business,
            membership.Role.Name);
    }

    // ---------------------------------------------------------
    // UPDATE BUSINESS
    // ---------------------------------------------------------

    public async Task<BusinessResponse> UpdateBusinessAsync(
        Guid userId,
        Guid businessId,
        UpdateBusinessRequest request,
        CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        ValidateBusinessId(businessId);

        ArgumentNullException.ThrowIfNull(request);

        var membership = await GetRequiredMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        EnsureCanManageBusiness(membership.Role.Name);

        var business = membership.Business;

        if (request.Name is not null)
        {
            var name = NormalizeRequired(
                request.Name,
                "Business name");

            if (name.Length > 150)
            {
                throw new ArgumentException(
                    "Business name cannot exceed 150 characters.");
            }

            business.Name = name;
        }

        if (request.Description is not null)
        {
            business.Description =
                NormalizeOptional(request.Description);
        }

        if (request.Email is not null)
        {
            business.Email = NormalizeOptional(request.Email)
                ?.ToLowerInvariant();
        }

        if (request.PhoneNumber is not null)
        {
            business.PhoneNumber =
                NormalizeOptional(request.PhoneNumber);
        }

        if (request.Address is not null)
        {
            business.Address =
                NormalizeOptional(request.Address);
        }

        if (request.City is not null)
        {
            business.City = NormalizeOptional(request.City);
        }

        if (request.State is not null)
        {
            business.State = NormalizeOptional(request.State);
        }

        if (request.CountryCode is not null)
        {
            business.CountryCode = NormalizeCode(
                request.CountryCode,
                "NG",
                2);
        }

        if (request.CurrencyCode is not null)
        {
            business.CurrencyCode = NormalizeCode(
                request.CurrencyCode,
                "NGN",
                3);
        }

        if (request.TimeZoneId is not null)
        {
            business.TimeZoneId =
                NormalizeOptional(request.TimeZoneId);
        }

        if (request.LogoUrl is not null)
        {
            business.LogoUrl =
                NormalizeOptional(request.LogoUrl);
        }

        business.UpdatedAt = DateTime.UtcNow;

        await businessRepo.SaveChangesAsync(cancellationToken);

        // Business details appear in users' business lists.
        await InvalidateBusinessCacheAsync(cancellationToken);

        return MapToResponse(
            business,
            membership.Role.Name);
    }

    // ---------------------------------------------------------
    // DEACTIVATE BUSINESS
    // ---------------------------------------------------------

    public async Task DeactivateBusinessAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var membership = await GetRequiredMembershipAsync(
            userId,
            businessId,
            cancellationToken);

        if (!string.Equals(
            membership.Role.Name,
            BusinessRoles.Owner,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(
                "Only the business Owner can deactivate this business.");
        }

        membership.Business.IsActive = false;
        membership.Business.UpdatedAt = DateTime.UtcNow;

        await businessRepo.SaveChangesAsync(cancellationToken);

        // Remove cached lists containing this business.
        await InvalidateBusinessCacheAsync(cancellationToken);
    }

    // ---------------------------------------------------------
    // MEMBERSHIP AND AUTHORIZATION
    // ---------------------------------------------------------

    private async Task<BusinessUser> GetRequiredMembershipAsync(
        Guid userId,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        ValidateBusinessId(businessId);

        return await businessRepo.GetMembershipAsync(
            businessId,
            userId,
            cancellationToken)
            ?? throw new NotFoundException(
                "Business was not found or you are not an active member.");
    }

    private static void EnsureCanManageBusiness(string role)
    {
        var canManage =
            string.Equals(
                role,
                BusinessRoles.Owner,
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                role,
                BusinessRoles.Manager,
                StringComparison.OrdinalIgnoreCase);

        if (!canManage)
        {
            throw new ForbiddenException(
                "You do not have permission to update this business.");
        }
    }

    // ---------------------------------------------------------
    // SLUG GENERATION
    // ---------------------------------------------------------

    private async Task<string> GenerateUniqueSlugAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var baseSlug = Regex.Replace(
            name.ToLowerInvariant(),
            @"[^a-z0-9]+",
            "-").Trim('-');

        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = "business";
        }

        // Add a suffix so businesses with the same name
        // can still have unique slugs.
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = Guid.NewGuid()
                .ToString("N")[..8];

            var slug = $"{baseSlug}-{suffix}";

            if (slug.Length > 160)
            {
                slug = slug[..160];
            }

            if (!await businessRepo.SlugExistsAsync(
                slug,
                cancellationToken))
            {
                return slug;
            }
        }

        throw new InvalidOperationException(
            "Unable to generate a unique business slug.");
    }

    // ---------------------------------------------------------
    // CACHE INVALIDATION
    // ---------------------------------------------------------

    private Task InvalidateBusinessCacheAsync(
        CancellationToken cancellationToken)
    {
        // Every cached business list contains business details.
        // Invalidate the shared tag after a business is updated
        // or deactivated.
        return cache.RemoveByTagAsync(
            CacheTags.Businesses,
            cancellationToken);
    }

    // ---------------------------------------------------------
    // VALIDATION
    // ---------------------------------------------------------

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        }
    }

    private static void ValidateBusinessId(Guid businessId)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException(
                "Business ID is required.",
                nameof(businessId));
        }
    }

    private static string NormalizeRequired(
        string? value,
        string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{field} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var normalized = value.Trim();

        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized;
    }

    private static string NormalizeCode(
        string? value,
        string defaultValue,
        int requiredLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != requiredLength ||
            !normalized.All(char.IsLetter))
        {
            throw new ArgumentException(
                $"The supplied code must contain exactly {requiredLength} letters.");
        }

        return normalized;
    }

    // ---------------------------------------------------------
    // RESPONSE MAPPING
    // ---------------------------------------------------------

    private static BusinessResponse MapToResponse(
        Business business,
        string role)
    {
        return new BusinessResponse(
            Id: business.Id,
            Name: business.Name,
            Slug: business.Slug,
            Description: business.Description,
            Email: business.Email,
            PhoneNumber: business.PhoneNumber,
            Address: business.Address,
            City: business.City,
            State: business.State,
            CountryCode: business.CountryCode,
            CurrencyCode: business.CurrencyCode,
            TimeZoneId: business.TimeZoneId,
            LogoUrl: business.LogoUrl,
            IsActive: business.IsActive,
            CreatedAt: business.CreatedAt,
            UpdatedAt: business.UpdatedAt,
            YourRole: role);
    }
}
