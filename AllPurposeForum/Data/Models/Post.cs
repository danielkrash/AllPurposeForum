namespace AllPurposeForum.Data.Models;

public partial class Post : BaseModel
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string ApplicationUserId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public bool NSFW { get; set; }
    
    public ApplicationUser ApplicationUser { get; set; }

    public ICollection<PostComment> PostComments { get; set; } =
        new HashSet<PostComment>(ReferenceEqualityComparer.Instance);
    public Topic Topic { get; set; }
}