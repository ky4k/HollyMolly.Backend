using HM.BLL.Enums;
using HM.BLL.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Models.Supports
{
    public class SupportDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public SupportTopicDto Topic { get; set; } 
        public string Description { get; set; } = null!;
        public string? OrderNumber { get; set; } 
        public OrderDto? Order { get; set; } 
    }
}
