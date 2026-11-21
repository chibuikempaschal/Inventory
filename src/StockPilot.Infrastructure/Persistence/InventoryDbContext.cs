
using Microsoft.EntityFrameworkCore;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Business> Businesses { get; set; }

    public DbSet<BusinessUser> BusinessUsers { get; set; }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ProductSupplier> ProductSuppliers => Set<ProductSupplier>();

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockAlert> StockAlerts => Set<StockAlert>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InventoryDbContext).Assembly);
    }
}
