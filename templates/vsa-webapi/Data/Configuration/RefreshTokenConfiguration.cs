#if AUTH_IDENTITY
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VsaTemplate.Data.Entities;

namespace VsaTemplate.Data.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Token).IsUnique();
        builder.HasIndex(e => e.UserId);

        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.ExpiresAt).IsRequired();
    }
}
#endif
