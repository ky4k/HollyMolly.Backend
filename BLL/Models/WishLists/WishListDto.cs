using HM.BLL.Models.Products;

namespace HM.BLL.Models.WishLists;

public class WishListDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public List<ProductDto> Products { get; set; } = [];
}
