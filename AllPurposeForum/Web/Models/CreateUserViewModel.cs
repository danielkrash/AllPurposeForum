using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AllPurposeForum.Web.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        // All available roles
        public List<string?> Roles { get; set; } = new List<string?>();

        // Roles to assign to the new user
        [Display(Name = "Assign Roles")]
        public List<string?> UserRoles { get; set; } = new List<string?>();
    }
}
