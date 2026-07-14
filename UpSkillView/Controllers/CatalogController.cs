using Application.CourseContent.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http.Headers;

namespace UpSkillView.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CatalogController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string? search = null, int? categoryId = null)
        {
            var client = _httpClientFactory.CreateClient("UpSkillAPI");
            
            // Add token if user is logged in
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var queryStr = $"?search={search}&categoryId={categoryId}";
            var response = await client.GetAsync($"api/StudentLearning/courses{queryStr}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<StudentCatalogCourseDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses ?? new List<StudentCatalogCourseDto>());
            }

            return View(new List<StudentCatalogCourseDto>());
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("UpSkillAPI");
            
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"api/StudentLearning/courses/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var details = JsonSerializer.Deserialize<StudentCourseDetailsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(details);
            }

            return RedirectToAction("Index");
        }
    }
}
