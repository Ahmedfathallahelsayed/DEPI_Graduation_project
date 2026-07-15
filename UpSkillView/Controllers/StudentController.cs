using Application.CourseContent.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace UpSkillView.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("UpSkillAPI");
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<IActionResult> Dashboard()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/studentflow/my-courses");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<Application.Courses.DTOs.Student.EnrollmentResponseDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses ?? new List<Application.Courses.DTOs.Student.EnrollmentResponseDto>());
            }

            return View(new List<Application.Courses.DTOs.Student.EnrollmentResponseDto>());
        }

        public async Task<IActionResult> MyCourses()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/studentflow/my-courses");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<Application.Courses.DTOs.Student.EnrollmentResponseDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses ?? new List<Application.Courses.DTOs.Student.EnrollmentResponseDto>());
            }

            return View(new List<Application.Courses.DTOs.Student.EnrollmentResponseDto>());
        }

        public async Task<IActionResult> CoursePlayer(int id)
        {
            var client = GetAuthenticatedClient();
            var hierarchyResponse = await client.GetAsync($"api/StudentLearning/course/{id}/hierarchy");
            var detailsResponse = await client.GetAsync($"api/StudentLearning/courses/{id}");
            var progressResponse = await client.GetAsync($"api/studentflow/course/{id}/progress");

            if (hierarchyResponse.IsSuccessStatusCode && detailsResponse.IsSuccessStatusCode)
            {
                var hierarchyContent = await hierarchyResponse.Content.ReadAsStringAsync();
                var sections = JsonSerializer.Deserialize<List<SectionDto>>(hierarchyContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var detailsContent = await detailsResponse.Content.ReadAsStringAsync();
                var details = JsonSerializer.Deserialize<StudentCourseDetailsDto>(detailsContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                int enrollmentId = 0;
                if (progressResponse.IsSuccessStatusCode)
                {
                    var progressContent = await progressResponse.Content.ReadAsStringAsync();
                    var progressDto = JsonSerializer.Deserialize<Application.Courses.DTOs.Student.StudentCourseProgressDto>(progressContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    enrollmentId = progressDto?.EnrollmentId ?? 0;
                }

                var vm = new UpSkillView.Models.CoursePlayerViewModel
                {
                    CourseId = id,
                    EnrollmentId = enrollmentId,
                    CourseTitle = details?.Title ?? "Course",
                    ProgressPercentage = details?.ProgressPercent ?? 0,
                    Sections = sections ?? new List<SectionDto>()
                };
                return View(vm);
            }

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"api/StudentLearning/courses/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var details = JsonSerializer.Deserialize<StudentCourseDetailsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                // If course is free, we don't need checkout, but user might have hit this URL directly.
                // It's safer to just let them see the checkout screen with $0, or we can auto-enroll here.
                // We'll auto-redirect to Enroll if Price == 0 to be safe.
                if (details?.Price == 0)
                {
                    // Auto submit to Enroll via POST? 
                    // Better to just let the View handle the $0 display, but since user said 
                    // "لو مجاني يسجل مباشره", the Catalog view will directly POST. 
                    // If they somehow land here, we can show it as $0.
                }

                return View(details);
            }

            return RedirectToAction("Details", "Catalog", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"api/studentflow/enroll/{courseId}", null);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CoursePlayer", new { id = courseId });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorContent) ? "Failed to enroll in the course." : errorContent;
            return RedirectToAction("Details", "Catalog", new { id = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkLessonComplete(int lessonId, int courseId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"api/studentflow/lesson/{lessonId}/complete", null);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorContent) ? "Failed to mark lesson complete." : errorContent;
            }

            return RedirectToAction("CoursePlayer", new { id = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadCertificate(int enrollmentId, int courseId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"api/studentflow/certificate/{enrollmentId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cert = JsonSerializer.Deserialize<Application.Courses.DTOs.Student.CertificateResponseDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                // For demonstration, we just return the URL or mock a file download
                if (cert != null && !string.IsNullOrEmpty(cert.CertificateUrl))
                {
                    // In a real app, this would return a PDF file
                    TempData["SuccessMessage"] = $"Certificate '{cert.CertificateCode}' generated! (URL: {cert.CertificateUrl})";
                    return RedirectToAction("CoursePlayer", new { id = courseId });
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorContent) ? "Certificate not available yet." : errorContent;
            return RedirectToAction("CoursePlayer", new { id = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> ViewCertificate(int enrollmentId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"api/studentflow/certificate/{enrollmentId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cert = JsonSerializer.Deserialize<Application.Courses.DTOs.Student.CertificateResponseDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (cert != null)
                {
                    var studentName = User.FindFirst("FullName")?.Value 
                                      ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value 
                                      ?? User.Identity?.Name 
                                      ?? "Valued Student";

                    ViewBag.StudentName = studentName;
                    return View(cert);
                }
            }

            TempData["ErrorMessage"] = "Certificate not found or not available yet.";
            return RedirectToAction("Dashboard");
        }
    }
}
