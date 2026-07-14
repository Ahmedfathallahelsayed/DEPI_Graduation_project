using System.ComponentModel.DataAnnotations;

namespace UpSkillView.Models.Auth
{
    public class RegisterViewModel
    {
        [Required]
        public string RoleOptions { get; set; } // "Student" or "Instructor"

        [Required]
        public string FullName { get; set; }

        public string Country { get; set; }
        
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string RegisterEmail { get; set; }

        [Required]
        public string RegisterPassword { get; set; }

        [Required]
        [Compare("RegisterPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
