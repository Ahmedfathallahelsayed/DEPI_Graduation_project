using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UpSkillView.Models.Instructor
{
    public class EditCourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string CourseTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subtitle is required")]
        public string CourseSubtitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string CourseDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Level is required")]
        public int Level { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 99999.99, ErrorMessage = "Price must be between 0 and 99999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Language is required")]
        public string Language { get; set; } = string.Empty;

        /// <summary>Optional new thumbnail — if null, existing image is kept.</summary>
        public IFormFile? CourseImage { get; set; }

        /// <summary>The current thumbnail URL, shown as a preview while editing.</summary>
        public string? ExistingThumbnailUrl { get; set; }
    }
}
