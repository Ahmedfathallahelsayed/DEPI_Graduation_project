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

        public async Task<IActionResult> Index(string? search = null, [FromQuery] List<int>? categoryIds = null, [FromQuery] List<Domain.Enum.CourseLevel>? levels = null, string? price = null)
        {
            var client = _httpClientFactory.CreateClient("UpSkillAPI");
            
            // Add token if user is logged in
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var queryStr = $"?search={search}&price={price}";
            if (categoryIds != null)
            {
                foreach (var catId in categoryIds)
                    queryStr += $"&categoryIds={catId}";
            }
            if (levels != null)
            {
                foreach (var lvl in levels)
                    queryStr += $"&levels={lvl}";
            }
            var response = await client.GetAsync($"api/StudentLearning/courses{queryStr}");
            
            // Fetch Categories
            var categoriesResponse = await client.GetAsync("api/category");

            var vm = new UpSkillView.Models.CatalogViewModel
            {
                SearchQuery = search,
                SelectedCategoryIds = categoryIds ?? new List<int>(),
                SelectedLevels = levels ?? new List<Domain.Enum.CourseLevel>(),
                SelectedPrice = price
            };

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                vm.Courses = JsonSerializer.Deserialize<List<StudentCourseSummaryDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<StudentCourseSummaryDto>();
            }

            if (categoriesResponse.IsSuccessStatusCode)
            {
                var catContent = await categoriesResponse.Content.ReadAsStringAsync();
                vm.Categories = JsonSerializer.Deserialize<List<Application.Courses.DTOs.Category.CategoryResponseDto>>(catContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Application.Courses.DTOs.Category.CategoryResponseDto>();
            }

            return View(vm);
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
