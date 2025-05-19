// Filepath: c:\Users\danik\source\repos\AllPurposeForum\AllPurposeForum\Web\Models\CreateCommentViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace AllPurposeForum.Web.Models
{
    public class CreateCommentViewModel
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Content { get; set; }
    }
}
