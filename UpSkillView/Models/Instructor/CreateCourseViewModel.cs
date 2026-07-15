using Domain.Enum;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UpSkillView.Models.Instructor
{
    public class CreateCourseViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string CourseTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subtitle is required")]
        [StringLength(300, MinimumLength = 10, ErrorMessage = "Subtitle must be between 10 and 300 characters")]
        public string CourseSubtitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [MinLength(20, ErrorMessage = "Description must be at least 20 characters")]
        public string CourseDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Please select a level")]
        [Range(0, 10, ErrorMessage = "Please select a valid level")]
        public int? Level { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 9999.99, ErrorMessage = "Price must be between $0 and $9,999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Please select a language")]
        public string Language { get; set; } = "English";

        // Optional — backend accepts courses without thumbnails
        public IFormFile? CourseImage { get; set; }
    }
}
