using System.ComponentModel.DataAnnotations;

namespace AllPurposeForum.Web.Models
{
    public class EditTopicViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
        public string Description { get; set; }

        [Display(Name = "NSFW (Not Safe For Work)")]
        public bool IsNsfw { get; set; }
    }
}
