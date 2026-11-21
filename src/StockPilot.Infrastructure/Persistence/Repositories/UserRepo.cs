using StockPilot.Application.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockPilot.Infrastructure.Persistence;
using StockPilot.Domain.Entities;
using StockPilot.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace StockPilot.Infrastructure.Persistence.Repositories
{
    public class UserRepo(InventoryDbContext dbContext) : IuserRepo
    {
        public async Task<List<GetAllUserResponse>> GetAllWithRolesAsync(CancellationToken cancellationToken)
        {
            return await dbContext.Users
                        .AsNoTracking()
                        .Select(u => new GetAllUserResponse(
                            u.Id,
                            u.FullName,
                            u.Email,
                            u.UserRoles.Select(ur => new UserRoleDto(
                                ur.Role.Name,
                                ur.Role.Description
                            )).ToList(),
                            u.IsActive
                        ))
                        .ToListAsync(cancellationToken);

        }

        public async Task<UserResponse?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await dbContext.Users
                       .AsNoTracking()
                       .Where(u => u.Id == userId)
                       .Select(u => new UserResponse(
                        u.Id,
                            u.FullName,
                            u.Username,
                            u.Email,
                            u.UserRoles.Select(ur => new UserRoleDto(
                                ur.Role.Name,
                                ur.Role.Description
                            )).ToList(),
                            u.IsActive
                       ))
                       .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<UserResponse?> UpdateProfileAsync(Guid userId, UpdateUser updateUser, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                       .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(updateUser.FullName))
                user.FullName = updateUser.FullName;
            else
                throw new ArgumentException("Full name is required.");

            if (!string.IsNullOrWhiteSpace(updateUser.Username))
                user.Username = updateUser.Username;
            else
                throw new ArgumentException("Username is required.");

            if (!string.IsNullOrWhiteSpace(updateUser.Email))
                user.Email = updateUser.Email;
            else
                throw new ArgumentException("Email is required.");

            await dbContext.SaveChangesAsync(cancellationToken);

            return new UserResponse(
              UserId: user.Id,
              FullName: user.FullName,
              Username: user.Username,
              Email: user.Email,
              Roles: user.UserRoles.Select(ur => new UserRoleDto(
                        ur.Role.Name,
                        ur.Role.Description))
                    .ToList(),
              IsActive: user.IsActive
            );
        }

        public async Task<AuthProfile?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await dbContext.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => new AuthProfile(
                        u.Id,
                        u.FullName,
                        u.Username,
                        u.Email,
                        u.UserRoles.Select(ur => new UserRoleDto(
                            ur.Role.Name,
                            ur.Role.Description
                        )).ToList(),
                        u.IsActive
                    ))
                    .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<UserResponse?> GetByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await dbContext.Users
                       .AsNoTracking()
                       .Where(u => u.Username == name || u.FullName == name)
                       .Select(u => new UserResponse(
                        u.Id,
                            u.FullName,
                            u.Username,
                            u.Email,
                            u.UserRoles.Select(ur => new UserRoleDto(
                                ur.Role.Name,
                                ur.Role.Description
                            )).ToList(),
                            u.IsActive
                       ))
                       .FirstOrDefaultAsync(cancellationToken);
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

    }
}