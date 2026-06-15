using HRPayroll.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .ValueGeneratedOnAdd();

        builder.Property(rt => rt.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(rt => rt.TokenHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(rt => rt.FamilyId)
            .HasMaxLength(128);

        builder.Property(rt => rt.ReplacedByTokenHash)
            .HasMaxLength(256);

        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();

        builder.HasIndex(rt => rt.FamilyId);
        builder.HasIndex(rt => rt.UserId);
    }
}
