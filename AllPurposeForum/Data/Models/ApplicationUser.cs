using Microsoft.AspNetCore.Identity;

namespace AllPurposeForum.Data.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<Topic> Topics { get; set; } = new HashSet<Topic>(ReferenceEqualityComparer.Instance);
    public ICollection<Post> Posts { get; set; } = new HashSet<Post>(ReferenceEqualityComparer.Instance);
    public ICollection<PostComment> PostComments { get; set; } = new HashSet<PostComment>(ReferenceEqualityComparer.Instance);
}