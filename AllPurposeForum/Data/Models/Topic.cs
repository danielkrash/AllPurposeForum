namespace AllPurposeForum.Data.Models;

public partial class Topic : BaseModel
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public bool NSFW { get; set; }
    
    
    public ApplicationUser ApplicationUser { get; set; } 
    public ICollection<Post> Posts { get;} = new HashSet<Post>(ReferenceEqualityComparer.Instance);
}