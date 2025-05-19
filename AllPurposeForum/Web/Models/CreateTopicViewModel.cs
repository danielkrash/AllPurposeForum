using System.ComponentModel.DataAnnotations;

namespace AllPurposeForum.Web.Models
{
    public class CreateTopicViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
        public string Description { get; set; }
        [Required]
        public bool isNswf { get; set; } = false;
    }
}
