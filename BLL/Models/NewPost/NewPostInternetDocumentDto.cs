using HM.BLL.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Models.NewPost
{
    public class NewPostInternetDocumentDto
    {
        public string Ref { get; set; } = string.Empty;
        public string CostOnSite { get; set; } = string.Empty;
        public string EstimatedDeliveryDate { get; set; } = string.Empty;
        public string IntDocNumber { get; set; } = string.Empty;
        public string TypeDocument { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public OrderDto? Order { get; set; }
    }
}
