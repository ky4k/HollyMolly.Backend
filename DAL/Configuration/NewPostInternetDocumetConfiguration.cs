using HM.DAL.Entities.NewPost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.DAL.Configuration
{
    public class NewPostInternetDocumetConfiguration: IEntityTypeConfiguration<NewPostInternetDocument>
    {
        public void Configure(EntityTypeBuilder<NewPostInternetDocument> builder)
        {
            builder.HasKey(doc => doc.Id);
            builder.HasOne(doc => doc.Order)
                .WithOne(order => order.NewPostInternetDocument)
                .HasForeignKey<NewPostInternetDocument>(doc => doc.OrderId);
        }
    }
}
