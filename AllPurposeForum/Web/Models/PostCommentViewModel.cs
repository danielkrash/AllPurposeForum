// Filepath: c:\Users\danik\source\repos\AllPurposeForum\AllPurposeForum\Web\Models\PostCommentViewModel.cs
namespace AllPurposeForum.Web.Models
{
    public class PostCommentViewModel
    {
        public int Id { get; set; } 
        public string? UserName { get; set; }
        public string? Content { get; set; }
        public string? CreatedAtFormatted { get; set; }
        public bool IsApproved { get; set; } 
    }
}
