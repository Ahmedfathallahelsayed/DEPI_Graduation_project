using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UpSkillView.Models.Instructor
{
    public class CreateCourseViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string CourseTitle { get; set; }

        [Required(ErrorMessage = "Subtitle is required")]
        public string CourseSubtitle { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string CourseDescription { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Level is required")]
        public int Level { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Language is required")]
        public string Language { get; set; }

        [Required(ErrorMessage = "Course image is required")]
        public IFormFile CourseImage { get; set; }
    }
}
