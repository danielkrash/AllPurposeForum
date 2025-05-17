using Microsoft.AspNetCore.Identity;

namespace AllPurposeForum.Data.Models;

public partial class PostComment : BaseModel
{
    public required string UserId { get; set; }
    public required string Content { get; set; }
    public bool? Acceptence { get; set; }
    public Post Post { get; set; }
    public ApplicationUser User { get; set; }
}