
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(category => category.Description)
            .HasMaxLength(500);

        builder.Property(category => category.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Required by the composite Product -> Category relationship.
        builder.HasAlternateKey(category => new
        {
            category.BusinessId,
            category.Id
        });

        // Category names must be unique within a business.
        builder.HasIndex(category => new
        {
            category.BusinessId,
            category.Name
        })
        .IsUnique();

        builder.HasOne(category => category.Business)
            .WithMany()
            .HasForeignKey(category => category.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
