namespace Application.Courses.DTOs.Category
{
    /// <summary>
    /// Returned when reading category data (GET operations).
    /// </summary>
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
