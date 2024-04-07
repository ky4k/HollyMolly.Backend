using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.DAL.Entiries
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<Product> Products { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public string Status { get; set; } = null!;
    }
}
