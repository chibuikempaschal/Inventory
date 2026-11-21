using Microsoft.Extensions.Configuration;
using StockPilot.Application.DTOs;
using StockPilot.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using StockPilot.Infrastructure.Persistence;
using StockPilot.Application.Interfaces;
using StockPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Infrastructure.Services
{
    public sealed class AuthService(InventoryDbContext dbContext, IConfiguration configuration) : IAuthService
    {
        public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
              string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Email and Password are required.");

            var email = request.Email.Trim();
            var user = await dbContext.Users.AsNoTracking()
             .Include(u => u.UserRoles)
             .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);


            if (user is null)
            {
                BCrypt.Net.BCrypt.Verify(request.Password, "$2a$11$dummyhashdummyhashdummyhashdummyhash");
                return null;
            }
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var auth = await CreateAuthResponseAsync(user, cancellationToken);

            return new AuthResponse(
     auth.UserId, auth.FullName, auth.Email, auth.IsActive,
     auth.AccessToken, auth.AccessTokenExpiresAt,
     auth.RefreshToken, auth.RefreshTokenExpiresAt);
        }
        public async Task<AuthResponse?> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException(
                    "Email, Password and FullName are required.");
            }

            var email = request.Email.Trim().ToLowerInvariant();

            if (await dbContext.Users.AnyAsync(
                user => user.Email == email,
                cancellationToken))
            {
                throw new AlreadyExistException(email);
            }

            // // Generate a unique username from the email address.
            // var emailUsername = email.Split('@')[0];

            // var username = emailUsername;
            // var counter = 1;

            // while (await dbContext.Users.AnyAsync(
            //     user => user.Username == username,
            //     cancellationToken))
            // {
            //     username = $"{emailUsername}{counter}";
            //     counter++;
            // }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(
                request.Password);

            var createUser = new User
            {
                FullName = request.FullName.Trim(),
                // Username = username,
                Email = email,
                PasswordHash = hashedPassword
            };

            dbContext.Users.Add(createUser);

            await dbContext.SaveChangesAsync(cancellationToken);

            return await CreateAuthResponseAsync(
                createUser,
                cancellationToken);
        }

        public async Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken)
        {
            // 1. Find all active refresh tokens for this user
            var tokens = await dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync(cancellationToken);

            if (!tokens.Any())
                return false; // Already logged out

            // 2. Delete them all
            dbContext.RefreshTokens.RemoveRange(tokens);
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null) return false;

            var refreshToken = await dbContext.RefreshTokens
                    .Where(rt => rt.UserId == userId)
                    .ToListAsync(cancellationToken);

            if (refreshToken.Any())
            {
                dbContext.RefreshTokens.RemoveRange(refreshToken);
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
        private async Task<AuthResponse> CreateAuthResponseAsync(
            User user,
            CancellationToken cancellationToken)
        {
            var issuer = configuration["AppSettings:Issuer"]
                ?? throw new InvalidOperationException("Jwt Issuer is missing");

            var audience = configuration["AppSettings:Audience"]
                ?? throw new InvalidOperationException("Jwt Audience is missing");

            var secretKey = configuration["AppSettings:SecretKey"]
                ?? throw new InvalidOperationException("Jwt Secret key is missing");

            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(
                GetTokenExpirationMinutes());

            // Get the active business memberships belonging to this user.
            var businessMemberships = await dbContext.Set<BusinessUser>()
                .AsNoTracking()
                .Include(bu => bu.Business)
                .Where(bu =>
                    bu.UserId == user.Id &&
                    bu.Business.IsActive)
                .ToListAsync(cancellationToken);

            var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.FullName),
        new(ClaimTypes.GivenName, user.Username ?? ""),
        new(ClaimTypes.Email, user.Email)
    };

            // Add BusinessId only when the user has exactly one active membership.
            // Users with multiple businesses will need a business-selection flow.
            if (businessMemberships.Count == 1)
            {
                var membership = businessMemberships[0];

                claims.Add(new Claim(
                    "BusinessId",
                    membership.BusinessId.ToString()));
            }

            // Preserve the existing global role claims for now.
            if (user.UserRoles != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    claims.Add(new Claim(
                        ClaimTypes.Role,
                        userRole.Role.Name));
                }
            }

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            if (keyBytes.Length < 64)
                throw new InvalidOperationException(
                    "SecretKey must be at least 64 bytes for HS512.");

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha512);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt.UtcDateTime,
                signingCredentials: signingCredentials);

            var accessToken = new JwtSecurityTokenHandler()
                .WriteToken(jwt);

            var (rawRefreshToken, refreshExpiresAt) =
                GenerateRefreshToken();

            dbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashRefreshToken(rawRefreshToken),
                ExpiresAt = refreshExpiresAt,
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            return new AuthResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.IsActive,
                accessToken,
                expiresAt,
                rawRefreshToken,
                refreshExpiresAt);
        }

        public (string token, DateTime expiresAt) GenerateRefreshToken()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenExpiryDays = configuration.GetValue<int?>("AppSettings:RefreshTokenExpirationInDays")
     ?? throw new InvalidOperationException("RefreshTokenExpirationInDays is missing");
            if (refreshTokenExpiryDays <= 0)
                throw new InvalidOperationException("RefreshTokenExpirationInDays must be positive");
            var expiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);
            return (token, expiresAt);
        }

        public DateTime GetAccessTokenExpiry() =>
            DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes());

        private int GetTokenExpirationMinutes()
        {
            var minutes = configuration.GetValue<int>("AppSettings:TokenExpirationInMinutes", 60);
            if (minutes <= 0)
                throw new InvalidOperationException("Expiration must be a positive number");
            return minutes;
        }

        private static string HashRefreshToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToBase64String(bytes);
        }
    }
}