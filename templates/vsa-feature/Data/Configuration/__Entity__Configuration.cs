using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using __RootNamespace__.Data.Entities.__Category__;

namespace __RootNamespace__.Data.Configuration;

public class __Entity__Configuration : IEntityTypeConfiguration<__Entity__>
{
    public void Configure(EntityTypeBuilder<__Entity__> builder)
    {
        builder.ToTable("__resource__");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);
    }
}
