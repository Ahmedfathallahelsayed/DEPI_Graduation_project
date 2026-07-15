using Domain.Enum;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Courses.DTOs.Course
{
    /// <summary>
    /// Used when an instructor creates a new course (POST).
    /// ThumbnailFile is the uploaded image file from the form.
    /// </summary>
    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Short description is required.")]
        [MaxLength(500, ErrorMessage = "Short description cannot exceed 500 characters.")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, 99999.99, ErrorMessage = "Price must be between 0 and 99999.99.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Level is required.")]
        public CourseLevel Level { get; set; }

        [Required(ErrorMessage = "Language is required.")]
        [MaxLength(50, ErrorMessage = "Language cannot exceed 50 characters.")]
        public string Language { get; set; }

        /// <summary>
        /// Optional thumbnail image file uploaded via multipart/form-data.
        /// </summary>
        public IFormFile? ThumbnailFile { get; set; }
    }
}
