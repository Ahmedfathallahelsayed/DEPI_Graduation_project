using Application.Admin.DTOs;
using Application.Courses.DTOs.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace UpSkillView.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
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
            var response = await client.GetAsync("api/Admin/dashboard");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var stats = JsonSerializer.Deserialize<DashboardStatsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(stats);
            }

            return View(new DashboardStatsDto());
        }

        public async Task<IActionResult> Courses()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/Admin/pending-courses");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<CourseResponseDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses ?? new List<CourseResponseDto>());
            }

            return View(new List<CourseResponseDto>());
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var client = GetAuthenticatedClient();
            await client.PostAsync($"api/Admin/courses/{id}/approve", null);
            return RedirectToAction("Courses");
        }

        [HttpPost]
        public async Task<IActionResult> Publish(int id)
        {
            var client = GetAuthenticatedClient();
            await client.PostAsync($"api/Admin/courses/{id}/publish", null);
            return RedirectToAction("Courses");
        }
        [HttpGet]
        public IActionResult Users()
        {
            // For MVP, just return view. Real implementation would fetch users.
            return View();
        }

        [HttpGet]
        public IActionResult Categories()
        {
            // For MVP, just return view. Real implementation would fetch categories.
            return View();
        }
    }
}
