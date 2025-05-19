// Filepath: c:\Users\danik\source\repos\AllPurposeForum\AllPurposeForum\Web\Models\CreatePostViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace AllPurposeForum.Web.Models
{
    public class CreatePostViewModel
    {
        [Required]
        public int TopicId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [StringLength(10000, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 10)]
        public string Content { get; set; }

        // Optional: To display topic title on the create post page
        public string TopicTitle { get; set; }
    }
}
