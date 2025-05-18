namespace AllPurposeForum.Data.DTO;

public class UpdatePostCommentDTO
{
    public int CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
}