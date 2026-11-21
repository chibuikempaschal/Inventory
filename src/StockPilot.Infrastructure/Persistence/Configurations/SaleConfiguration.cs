
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration
    : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ReceiptNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.SaleDate)
            .IsRequired();

        builder.Property(s => s.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(s => s.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(s => s.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(s => s.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.ReceiptNumber)
            .IsUnique();

        builder.HasOne(s => s.Business)
.WithMany()
.HasForeignKey(s => s.BusinessId)
.OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new
        {
            s.BusinessId,
            s.ReceiptNumber
        }).IsUnique();
    }
}
