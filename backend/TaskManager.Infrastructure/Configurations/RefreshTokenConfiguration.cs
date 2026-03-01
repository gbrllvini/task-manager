using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Infrastructure.Identity;

namespace TaskManager.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken> {
    public void Configure(EntityTypeBuilder<RefreshToken> builder) {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAtUtc)
            .IsRequired();

        builder.Property(rt => rt.CreatedAtUtc)
            .IsRequired();

        builder.Property(rt => rt.ReplacedByTokenHash)
            .HasMaxLength(128);

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(64);

        builder.Property(rt => rt.RevokedByIp)
            .HasMaxLength(64);

        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();
    }
}