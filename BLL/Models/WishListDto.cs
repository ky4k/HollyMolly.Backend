namespace HM.BLL.Models;

public class WishListDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
}
