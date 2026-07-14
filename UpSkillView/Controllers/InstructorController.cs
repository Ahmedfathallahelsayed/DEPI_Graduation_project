using Application.Courses.DTOs.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace UpSkillView.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public InstructorController(IHttpClientFactory httpClientFactory)
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
            // For now, we can just display the courses on the dashboard as well
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/course/my-courses");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<CourseResponseDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses ?? new List<CourseResponseDto>());
            }

            return View(new List<CourseResponseDto>());
        }

        public async Task<IActionResult> MyCourses()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/course/my-courses");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<CourseResponseDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses ?? new List<CourseResponseDto>());
            }

            return View(new List<CourseResponseDto>());
        }

        [HttpPost]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"api/course/{id}/submit-for-review", null);

            return RedirectToAction("MyCourses");
        }

        [HttpGet]
        public IActionResult CreateCourse()
        {
            return View();
        }
    }
}
