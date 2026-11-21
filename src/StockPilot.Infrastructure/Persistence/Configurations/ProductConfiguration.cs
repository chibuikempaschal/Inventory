
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.SellingPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.QuantityInStock)
            .IsRequired();

        builder.Property(p => p.ReorderLevel)
            .IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // SKU must be unique within each business.
        builder.HasIndex(p => new { p.BusinessId, p.SKU })
            .IsUnique();

        builder.HasIndex(p => new { p.BusinessId, p.Name });

        // Business relationship.
        builder.HasOne(p => p.Business)
            .WithMany()
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        // Product must reference a category
        // belonging to the same business.
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => new { p.BusinessId, p.CategoryId })
            .HasPrincipalKey(c => new { c.BusinessId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Products", table =>
        {
            table.HasCheckConstraint(
                "CK_Products_CostPrice_NonNegative",
                "\"CostPrice\" >= 0");

            table.HasCheckConstraint(
                "CK_Products_SellingPrice_NonNegative",
                "\"SellingPrice\" >= 0");

            table.HasCheckConstraint(
                "CK_Products_QuantityInStock_NonNegative",
                "\"QuantityInStock\" >= 0");

            table.HasCheckConstraint(
                "CK_Products_ReorderLevel_NonNegative",
                "\"ReorderLevel\" >= 0");
        });
    }
}
