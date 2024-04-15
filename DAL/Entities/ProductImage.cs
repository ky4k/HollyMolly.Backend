namespace HM.DAL.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductInstanceId { get; set; }
    public int Position { get; set; }
    public string FilePath { get; set; } = null!;
    public string Link { get; set; } = null!;
    public ProductInstance ProductInstance { get; set; } = null!;
}
