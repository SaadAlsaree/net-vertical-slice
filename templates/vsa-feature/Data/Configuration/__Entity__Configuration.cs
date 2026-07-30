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

        // TODO(__Entity__): configure your table columns, indexes, and relationships here.
        // Example:
        //   builder.HasIndex(e => e.Code).IsUnique();
        //   builder.Property(e => e.Code).IsRequired().HasMaxLength(10);
        //   builder.Property(e => e.NameAr).IsRequired().HasMaxLength(50);
        //   builder.Property(e => e.NameEn).HasMaxLength(50);
    }
}
