using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HM.DAL.Configuration;

public class ProductInstanceConfiguration : IEntityTypeConfiguration<ProductInstance>
{
    public void Configure(EntityTypeBuilder<ProductInstance> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Price)
            .HasPrecision(10, 2);
        builder.Property(d => d.AbsoluteDiscount)
            .HasPrecision(8, 2);
        builder.Property(d => d.PercentageDiscount)
            .HasPrecision(8, 4);
        builder.HasMany(p => p.Images)
            .WithOne(i => i.ProductInstance)
            .HasForeignKey(i => i.ProductInstanceId);
        builder.HasMany(p => p.OrderRecords)
            .WithOne(o => o.ProductInstance)
            .HasForeignKey(o => o.ProductInstanceId);
        builder.HasMany(pi => pi.ProductInstanceStatistics)
            .WithOne(pis => pis.ProductInstance)
            .HasForeignKey(pis => pis.ProductInstanceId);
    }
}
