namespace AllPurposeForum.Web.Models;

public class PostViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string UserName { get; set; }
    public string CreatedAtFormatted { get; set; }
    public int CommentsCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}
