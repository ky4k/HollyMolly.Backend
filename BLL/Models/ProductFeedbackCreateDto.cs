namespace HM.BLL.Models;

public class ProductFeedbackCreateDto
{
    public string AuthorName { get; set; } = string.Empty;
    public string? Review { get; set; }
    public int Rating { get; set; }
}
