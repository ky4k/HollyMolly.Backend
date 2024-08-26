using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.DAL.Entities.NewPost
{
    public class NewPostInternetDocument
    {
        public int Id { get; set; }
        public string Ref { get; set; } = string.Empty;
        public string CostOnSite = string.Empty;
        public string EstimatedDeliveryDate { get; set; } = string.Empty;
        public string IntDocNumber {  get; set; } = string.Empty;
        public string TypeDocument {  get; set; } = string.Empty;
        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
