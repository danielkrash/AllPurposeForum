namespace AllPurposeForum.Data.DTO;

public class CreatePostCommentDTO
{
    public string UserId { get; set; }
    public string Content { get; set; }
    public int PostId { get; set; }
}