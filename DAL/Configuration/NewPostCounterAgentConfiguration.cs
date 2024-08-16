using HM.DAL.Entities;
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
    public class NewPostCounterAgentConfiguration: IEntityTypeConfiguration<NewPostCounterAgent>
    {
        public void Configure(EntityTypeBuilder<NewPostCounterAgent> builder)
        {
            builder.HasKey(ca => ca.Id);

            builder.HasMany(ca => ca.ContactPersons)
                .WithOne(cp => cp.CounterAgent)
                .HasForeignKey(cp => cp.CounterpartyRef);
        }
    }
}
