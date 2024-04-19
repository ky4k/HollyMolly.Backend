namespace HM.DAL.Entities
{
    public class WishList
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
