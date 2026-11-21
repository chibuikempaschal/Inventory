
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockPilot.Domain.Entities;

namespace StockPilot.Infrastructure.Persistence.Configurations;

public sealed class BusinessUserConfiguration
    : IEntityTypeConfiguration<BusinessUser>
{
    public void Configure(EntityTypeBuilder<BusinessUser> builder)
    {
        builder.ToTable("BusinessUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(x => x.Business)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // A user can only have one membership in a particular business.
        builder.HasIndex(x => new
        {
            x.BusinessId,
            x.UserId
        }).IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.RoleId);
    }
}
