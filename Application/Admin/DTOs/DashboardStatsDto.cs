namespace Application.Admin.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalCourses { get; set; }
        public int PendingCoursesCount { get; set; }
        public int PublishedCoursesCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
