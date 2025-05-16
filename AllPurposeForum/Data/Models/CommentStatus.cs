namespace AllPurposeForum.Data.Models;

public partial class CommentStatus : BaseModel
{
    public string Status { get; set; } = null!;
    public string? Description { get; set; }
    
    public virtual ICollection<PostComment> PostComments { get; set; } = new HashSet<PostComment>(ReferenceEqualityComparer.Instance);
}