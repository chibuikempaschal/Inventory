
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class SaleItemConfiguration
    : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(si => si.Id);

        builder.Property(si => si.Quantity)
            .IsRequired();

        builder.Property(si => si.UnitCostPrice)
            .HasPrecision(18, 2);

        builder.Property(si => si.UnitSellingPrice)
            .HasPrecision(18, 2);

        builder.Property(si => si.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(si => si.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(si => si.LineTotal)
            .HasPrecision(18, 2);

        builder.HasOne(si => si.Sale)
            .WithMany(s => s.SaleItems)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.Product)
            .WithMany(p => p.SaleItems)
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(si => si.SaleId);
        builder.HasIndex(si => si.ProductId);

        builder.ToTable("SaleItems", table =>
        {
            table.HasCheckConstraint(
                "CK_SaleItems_Quantity_Positive",
                "\"Quantity\" > 0");
        });
    }
}
