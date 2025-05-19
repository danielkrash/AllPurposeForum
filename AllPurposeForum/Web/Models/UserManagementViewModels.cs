using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AllPurposeForum.Web.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        // All available roles
        public List<string?> Roles { get; set; } = new List<string?>();

        // Roles assigned to the user
        [Display(Name = "Assigned Roles")]
        public List<string?> UserRoles { get; set; } = new List<string?>();
    }
}
