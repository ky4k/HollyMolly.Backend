using HM.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.DAL.Entities
{
    public class Support
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public SupportTopic Topic { get; set; } 
        public string Description { get; set; } = null!;
        public string? OrderNumber { get; set; } 
        public int? OrderId { get; set; } 
        public Order? Order { get; set; }
    }
}
