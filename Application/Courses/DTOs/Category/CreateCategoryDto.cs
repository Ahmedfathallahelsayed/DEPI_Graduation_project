using System.ComponentModel.DataAnnotations;

namespace Application.Courses.DTOs.Category
{
    /// <summary>
    /// Used when creating a new category (POST).
    /// </summary>
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }
    }
}
