namespace AllPurposeForum.Data.DTO;

public class PostCommentDTO
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Content { get; set; }
}