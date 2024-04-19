using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HM.DAL.Configuration;

public class ProductStatisticsConfiguration : IEntityTypeConfiguration<ProductStatistics>
{
    public void Configure(EntityTypeBuilder<ProductStatistics> builder)
    {
        builder.HasKey(ps => ps.Id);
        builder.HasMany(ps => ps.ProductInstanceStatistics)
            .WithOne(pi => pi.ProductStatistics)
            .HasForeignKey(pi => pi.ProductStatisticsId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
