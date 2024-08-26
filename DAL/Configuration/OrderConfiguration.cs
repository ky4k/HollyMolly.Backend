using HM.DAL.Entities;
using HM.DAL.Entities.NewPost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HM.DAL.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.HasOne(o => o.Customer)
            .WithOne(c => c.Order)
            .HasForeignKey<CustomerInfo>(c => c.OrderId);

        builder.HasOne(o => o.NewPostInternetDocument)
            .WithOne(doc => doc.Order)
            .HasForeignKey<NewPostInternetDocument>(doc => doc.OrderId);
    }
}
