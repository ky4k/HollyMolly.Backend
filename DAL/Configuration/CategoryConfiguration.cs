using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HM.DAL.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasMany(c => c.Products)
            .WithOne(c => c.Category)
            .HasForeignKey(p => p.CategoryId);
        builder.HasOne(c => c.CategoryGroup)
            .WithMany(cg => cg.Categories)
            .HasForeignKey(c => c.CategoryGroupId);
    }
}
