namespace HM.DAL.Entities.NewPost
{
    public class NewPostInternetDocument
    {
        public int Id { get; set; }
        public string Ref { get; set; } = string.Empty;
        public float CostOnSite { get; set; }
        public string EstimatedDeliveryDate { get; set; } = string.Empty;
        public string IntDocNumber {  get; set; } = string.Empty;
        public string TypeDocument {  get; set; } = string.Empty;
        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
