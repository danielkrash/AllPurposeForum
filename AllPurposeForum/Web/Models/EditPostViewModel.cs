using System.ComponentModel.DataAnnotations;

namespace AllPurposeForum.Web.Models
{
    public class EditPostViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [StringLength(10000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
        public string Content { get; set; }

        // For context and breadcrumbs/navigation
        public int TopicId { get; set; }
        public string? TopicTitle { get; set; }
        public string? OriginalPostTitlePreview { get; set; } // To show what post is being edited
    }
}
