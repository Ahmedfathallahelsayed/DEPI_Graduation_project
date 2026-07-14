namespace Application.Admin.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public int PendingCoursesCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
