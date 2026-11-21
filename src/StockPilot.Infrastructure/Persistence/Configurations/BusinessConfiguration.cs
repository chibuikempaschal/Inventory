
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class BusinessConfiguration
    : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("Businesses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Email)
            .HasMaxLength(254);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(x => x.Address)
            .HasMaxLength(300);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .HasMaxLength(100);

        builder.Property(x => x.CountryCode)
            .IsRequired()
            .HasMaxLength(2)
            .HasDefaultValue("NG");

        builder.Property(x => x.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("NGN");

        builder.Property(x => x.TimeZoneId)
            .HasMaxLength(100);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => x.Name);
    }
}
