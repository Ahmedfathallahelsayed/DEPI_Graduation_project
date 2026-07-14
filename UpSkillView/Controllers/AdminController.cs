using Application.Admin.DTOs;
using Application.Courses.DTOs.Category;
using Application.Courses.DTOs.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace UpSkillView.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly JsonSerializerOptions JsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("UpSkillAPI");
            var token  = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ══════════════════════════════════════════════════════════════
        // ADMIN DASHBOARD
        // ══════════════════════════════════════════════════════════════

        public async Task<IActionResult> Dashboard()
        {
            var client   = GetAuthenticatedClient();
            var response = await client.GetAsync("api/Admin/dashboard");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var stats   = JsonSerializer.Deserialize<DashboardStatsDto>(content, JsonOpts);
                return View(stats);
            }

            return View(new DashboardStatsDto());
        }

        // ══════════════════════════════════════════════════════════════
        // COURSE APPROVAL
        // ══════════════════════════════════════════════════════════════

        public async Task<IActionResult> Courses()
        {
            var client   = GetAuthenticatedClient();
            var response = await client.GetAsync("api/Admin/pending-courses");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<CourseResponseDto>>(content, JsonOpts);
                return View(courses ?? new List<CourseResponseDto>());
            }

            return View(new List<CourseResponseDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var client = GetAuthenticatedClient();
            var result = await client.PostAsync($"api/Admin/courses/{id}/approve", null);

            if (result.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Course approved successfully.";
            else
                TempData["ErrorMessage"] = "Failed to approve course.";

            return RedirectToAction("Courses");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var client = GetAuthenticatedClient();
            var result = await client.PostAsync($"api/Admin/courses/{id}/publish", null);

            if (result.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Course published successfully.";
            else
                TempData["ErrorMessage"] = "Failed to publish course.";

            return RedirectToAction("Courses");
        }

        // ══════════════════════════════════════════════════════════════
        // USERS (stub — Member 1 owns this)
        // ══════════════════════════════════════════════════════════════

        [HttpGet]
        public IActionResult Users()
        {
            return View();
        }

        // ══════════════════════════════════════════════════════════════
        // CATEGORY MANAGEMENT
        // ══════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var client   = GetAuthenticatedClient();
            var response = await client.GetAsync("api/category/all");

            List<CategoryResponseDto> categories = new();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                categories = JsonSerializer.Deserialize<List<CategoryResponseDto>>(json, JsonOpts)
                             ?? new List<CategoryResponseDto>();
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to load categories.";
            }

            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Category name is required.";
                return RedirectToAction("Categories");
            }

            var client  = GetAuthenticatedClient();
            var payload = new { Name = name.Trim(), Description = description?.Trim() ?? string.Empty };
            var json    = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/category", json);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = $"Category \"{name}\" created successfully.";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to create category: {error}";
            }

            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, string name, string? description, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Category name is required.";
                return RedirectToAction("Categories");
            }

            var client  = GetAuthenticatedClient();
            var payload = new { Name = name.Trim(), Description = description?.Trim() ?? string.Empty, IsActive = isActive };
            var json    = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/category/{id}", json);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = $"Category \"{name}\" updated successfully.";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to update category: {error}";
            }

            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var client   = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"api/category/{id}");

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Category deleted successfully.";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Could not delete category: {error}";
            }

            return RedirectToAction("Categories");
        }
    }
}
