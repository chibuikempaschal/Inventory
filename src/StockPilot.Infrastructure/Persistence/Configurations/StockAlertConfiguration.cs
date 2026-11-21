
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class StockAlertConfiguration
    : IEntityTypeConfiguration<StockAlert>
{
    public void Configure(EntityTypeBuilder<StockAlert> builder)
    {
        builder.ToTable("StockAlerts");

        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.QuantityAtAlert)
            .IsRequired();

        builder.Property(sa => sa.ReorderLevelAtAlert)
            .IsRequired();

        builder.Property(sa => sa.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(sa => sa.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(
                StockPilot.Domain.Enums.StockAlertStatus.Active);

        builder.Property(sa => sa.CreatedAt)
            .IsRequired();

        builder.HasOne(sa => sa.Product)
            .WithMany(p => p.StockAlerts)
            .HasForeignKey(sa => sa.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sa => sa.ProductId);
        builder.HasIndex(sa => new
        {
            sa.ProductId,
            sa.Status
        });
    }
}
