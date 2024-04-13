namespace HM.DAL.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Position { get; set; }
    public string FilePath { get; set; } = null!;
    public string Link { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
