using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HM.DAL.Configuration;

public class SupportConfiguration : IEntityTypeConfiguration<Support>
{
    public void Configure(EntityTypeBuilder<Support> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Order)
            .WithMany(o => o.Supports)
            .HasForeignKey(s => s.OrderId);
    }
}
