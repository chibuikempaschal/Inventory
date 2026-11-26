using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using StockPilot.Infrastructure.Persistence;
using StockPilot.Infrastructure.Persistence.Repositories;
using StockPilot.Infrastructure.Services;
using StockPilot.Application.Interfaces;
using StockPilot.Application.Interfaces.Repositories;
using StockPilot.Application.Features.Categories;
using StockPilot.Application.Features.Users;
using StockPilot.Application.Features.Products;
using StockPilot.Application.Features.Roles;
using StockPilot.Application.Interfaces.Services;
using StockPilot.Infrastructure.Caching;
using StockPilot.Application.Features.Businesses;

namespace StockPilot.Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Inventory")
            ?? throw new InvalidOperationException("Connection string 'Inventory' is not configured.");

        services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(connectionString));
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            };
        });

        services.AddScoped<IuserRepo, UserRepo>();
        services.AddScoped<IProductRepo, ProductRepo>();
        services.AddScoped<ICategoryRepo, CategoryRepo>();
        services.AddScoped<ISaleRepo, SaleRepo>();
        services.AddScoped<ISaleItemRepo, SaleItemRepo>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IApplicationCache, HybridApplicationCache>();
        services.AddScoped<IBusinessRepo, BusinessRepo>();
        services.AddScoped<IBusinessService, BusinessService>();

        services.AddScoped<IBusinessMemberService, BusinessMemberService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUser, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDbAdminInitializer, DbAdminInitializer>();
        return services;
    }
}
