namespace HM.DAL.Entities;

public class ProductFeedback
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }
    public string? Review { get; set; }
    public int Rating { get; set; }
}
