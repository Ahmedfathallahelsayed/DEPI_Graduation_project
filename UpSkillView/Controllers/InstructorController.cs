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

        [HttpPost]
        public async Task<IActionResult> CreateCourse(UpSkillView.Models.Instructor.CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = GetAuthenticatedClient();

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.CourseTitle ?? ""), "Title");
            content.Add(new StringContent(model.CourseSubtitle ?? ""), "ShortDescription");
            content.Add(new StringContent(model.CourseDescription ?? ""), "Description");
            content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
            content.Add(new StringContent(model.Level.ToString()), "Level");
            content.Add(new StringContent(model.Price.ToString()), "Price");
            content.Add(new StringContent(model.Language ?? "English"), "Language");

            if (model.CourseImage != null)
            {
                var streamContent = new StreamContent(model.CourseImage.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.CourseImage.ContentType);
                content.Add(streamContent, "ThumbnailFile", model.CourseImage.FileName);
            }

            var response = await client.PostAsync("api/course", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course created successfully!";
                return RedirectToAction("MyCourses");
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Failed to create course. Error: {errorResponse}");
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageContent(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"api/CourseContent/course/{id}/sections");
            
            var vm = new UpSkillView.Models.Instructor.ManageContentViewModel { CourseId = id };
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var sections = JsonSerializer.Deserialize<List<Application.CourseContent.DTOs.SectionDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (sections != null)
                {
                    vm.Sections = sections;
                }
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSection([FromBody] Application.CourseContent.DTOs.CreateSectionDto dto)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync("api/CourseContent/sections", dto);
            if (response.IsSuccessStatusCode)
                return Ok();
            return BadRequest(await response.Content.ReadAsStringAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateLesson([FromBody] Application.CourseContent.DTOs.CreateLessonDto dto)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync("api/CourseContent/lessons", dto);
            if (response.IsSuccessStatusCode)
                return Ok();
            return BadRequest(await response.Content.ReadAsStringAsync());
        }
    }
}
