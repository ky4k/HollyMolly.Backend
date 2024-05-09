namespace HM.BLL.Models.NewPost;

public class NewPostResponse<T>
{
    public IEnumerable<T>? Results { get; set; }
}
