
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration
    : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(StockPilot.Domain.Enums.PaymentStatus.Pending);

        builder.Property(p => p.TransactionReference)
            .HasMaxLength(150);

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.HasOne(p => p.Sale)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.SaleId);

        builder.HasIndex(p => p.TransactionReference)
            .IsUnique()
            .HasFilter("\"TransactionReference\" IS NOT NULL");

        builder.ToTable("Payments", table =>
        {
            table.HasCheckConstraint(
                "CK_Payments_Amount_Positive",
                "\"Amount\" > 0");
        });
    }
}
