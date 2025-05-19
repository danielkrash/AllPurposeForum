using System.ComponentModel.DataAnnotations;

namespace AllPurposeForum.Web.Models
{
    public class EditCommentViewModel
    {
        public int Id { get; set; } // Comment ID

        [Required]
        public int PostId { get; set; } // To redirect back to the correct post or for context

        [Required(ErrorMessage = "Comment content cannot be empty.")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage = "Comment must be between {2} and {1} characters long.")]
        public string? Content { get; set; } // Made nullable

        // To carry over the approval status so it's not accidentally changed
        public bool IsApproved { get; set; }

        // For providing context in the Edit view
        public string? PostTitle { get; set; }
        public string? OriginalCommentContentPreview { get; set; }
    }
}
