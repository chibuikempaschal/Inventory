
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration
    : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.ContactPerson)
            .HasMaxLength(150);

        builder.Property(s => s.Email)
            .HasMaxLength(254);

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.Email);

        builder.HasOne(s => s.Business)
    .WithMany()
    .HasForeignKey(s => s.BusinessId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.BusinessId, s.Name });
    }
}
