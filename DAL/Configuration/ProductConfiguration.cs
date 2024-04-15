using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HM.DAL.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Rating)
            .HasPrecision(6, 4);
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(i => i.CategoryId);
        builder.HasOne(p => p.ProductStatistics)
            .WithOne(ps => ps.Product)
            .HasForeignKey<ProductStatistics>(ps => ps.ProductId);
        builder.HasMany(p => p.ProductInstances)
            .WithOne(pi => pi.Product)
            .HasForeignKey(pi => pi.ProductId);
    }
}
