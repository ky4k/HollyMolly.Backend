using HM.DAL.Entities.NewPost;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.DAL.Configuration
{
    public class NewPostContactPersonConfiguration : IEntityTypeConfiguration<NewPostContactPerson>
    {
        public void Configure(EntityTypeBuilder<NewPostContactPerson> builder)
        {
            builder.HasKey(cp => cp.Id);

            builder.HasOne(cp => cp.CounterAgent)
                .WithMany(ca => ca.ContactPersons)
                .HasForeignKey(cp => cp.CounterAgentId);
        }
    }
}
