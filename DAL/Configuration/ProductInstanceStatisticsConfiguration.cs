using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HM.DAL.Configuration;

public class ProductInstanceStatisticsConfiguration : IEntityTypeConfiguration<ProductInstanceStatistics>
{
    public void Configure(EntityTypeBuilder<ProductInstanceStatistics> builder)
    {
        builder.HasKey(pis => pis.Id);
        builder.Property(o => o.TotalRevenue)
            .HasPrecision(10, 2);
    }
}
