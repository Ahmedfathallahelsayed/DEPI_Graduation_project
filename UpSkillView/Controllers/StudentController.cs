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
            var response = await client.GetAsync("api/StudentLearning/my-courses");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<StudentEnrolledCourseDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses ?? new List<StudentEnrolledCourseDto>());
            }

            return View(new List<StudentEnrolledCourseDto>());
        }

        public async Task<IActionResult> MyCourses()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/StudentLearning/my-courses");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<StudentEnrolledCourseDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses ?? new List<StudentEnrolledCourseDto>());
            }

            return View(new List<StudentEnrolledCourseDto>());
        }

        public async Task<IActionResult> CoursePlayer(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"api/StudentLearning/course/{id}/hierarchy");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var hierarchy = JsonSerializer.Deserialize<CourseHierarchyDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(hierarchy);
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"api/StudentLearning/courses/{courseId}/enroll", null);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CoursePlayer", new { id = courseId });
            }

            // Ideally display an error message
            return RedirectToAction("Details", "Catalog", new { id = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkLessonComplete(int lessonId, int courseId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"api/StudentLearning/lessons/{lessonId}/complete", null);

            return RedirectToAction("CoursePlayer", new { id = courseId });
        }
    }
}
