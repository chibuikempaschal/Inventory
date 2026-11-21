
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class ProductSupplierConfiguration
    : IEntityTypeConfiguration<ProductSupplier>
{
    public void Configure(EntityTypeBuilder<ProductSupplier> builder)
    {
        builder.ToTable("ProductSuppliers");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.SupplierProductCode)
            .HasMaxLength(100);

        builder.Property(ps => ps.LastPurchasePrice)
            .HasPrecision(18, 2);

        builder.Property(ps => ps.LeadTimeDays);

        builder.HasOne(ps => ps.Product)
            .WithMany(p => p.ProductSuppliers)
            .HasForeignKey(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ps => ps.Supplier)
            .WithMany(s => s.ProductSuppliers)
            .HasForeignKey(ps => ps.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ps => new
        {
            ps.ProductId,
            ps.SupplierId
        }).IsUnique();
    }
}
