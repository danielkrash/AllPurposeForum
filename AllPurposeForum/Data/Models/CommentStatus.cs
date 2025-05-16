namespace AllPurposeForum.Data.Models;

public partial class CommentStatus : BaseModel
{
    public int CommentStatusId { get; set; }
    public string Status { get; set; } = null!;
    public string? Description { get; set; }
}