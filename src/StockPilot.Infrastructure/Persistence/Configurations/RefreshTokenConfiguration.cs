
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.ReplacedByTokenHash)
            .HasMaxLength(500);

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();

        builder.HasIndex(rt => rt.UserId);

        builder.HasIndex(rt => rt.ExpiresAt);
    }
}
