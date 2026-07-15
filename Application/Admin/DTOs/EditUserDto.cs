using System.ComponentModel.DataAnnotations;

namespace Application.Admin.DTOs
{
    public class EditUserDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }
    }
}
