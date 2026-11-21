
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class StockMovementConfiguration
    : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(sm => sm.QuantityChange)
            .IsRequired();

        builder.Property(sm => sm.QuantityBefore)
            .IsRequired();

        builder.Property(sm => sm.QuantityAfter)
            .IsRequired();

        builder.Property(sm => sm.Reason)
            .HasMaxLength(500);

        builder.Property(sm => sm.CreatedAt)
            .IsRequired();

        builder.HasOne(sm => sm.Product)
            .WithMany(p => p.StockMovements)
            .HasForeignKey(sm => sm.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.User)
            .WithMany()
            .HasForeignKey(sm => sm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.Sale)
            .WithMany(s => s.StockMovements)
            .HasForeignKey(sm => sm.SaleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(sm => sm.ProductId);
        builder.HasIndex(sm => sm.UserId);
        builder.HasIndex(sm => sm.CreatedAt);
        builder.HasIndex(sm => sm.SaleId);
    }
}
