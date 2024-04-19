namespace HM.DAL.Entities;

public class EmailLog
{
    public int Id { get; set; }
    public string RecipientEmail { get; set; } = null!;
    public string Subject { get; set; } = string.Empty;
    public DateTimeOffset SendAt { get; set; }
}
