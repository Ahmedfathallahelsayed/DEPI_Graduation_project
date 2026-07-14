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
                var courses = JsonSerializer.Deserialize<List<StudentCourseSummaryDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(courses ?? new List<StudentCourseSummaryDto>());
            }

            return View(new List<StudentCourseSummaryDto>());
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
            var hierarchyResponse = await client.GetAsync($"api/StudentLearning/course/{id}/hierarchy");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var details = JsonSerializer.Deserialize<StudentCourseDetailsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                List<SectionDto> sections = new List<SectionDto>();
                if (hierarchyResponse.IsSuccessStatusCode)
                {
                    var hContent = await hierarchyResponse.Content.ReadAsStringAsync();
                    sections = JsonSerializer.Deserialize<List<SectionDto>>(hContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<SectionDto>();
                }
                
                var vm = new UpSkillView.Models.CourseDetailsViewModel {
                    Details = details,
                    Sections = sections
                };
                return View(vm);
            }

            return RedirectToAction("Index");
        }
    }
}
