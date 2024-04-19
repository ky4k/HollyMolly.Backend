namespace HM.BLL.Models;

public class ProductFeedbackDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }
    public string? Review { get; set; }
    public int Rating { get; set; }
}
