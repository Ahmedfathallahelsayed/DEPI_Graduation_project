using Application.Courses.DTOs.Category;
using Application.Courses.DTOs.Course;
using Application.Courses.DTOs.Section;
using Application.Courses.DTOs.Lesson;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using UpSkillView.Models.Instructor;

namespace UpSkillView.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public InstructorController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // ── Authenticated HTTP client ──────────────────────────────────────────
        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("UpSkillAPI");
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ── API base URL (for thumbnail links) ────────────────────────────────
        private string ApiBaseUrl =>
            (_configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7209/").TrimEnd('/');

        // ── Category helper: populates ViewBag.Categories ─────────────────────
        private async Task LoadCategoriesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("UpSkillAPI");
                var response = await client.GetAsync("api/category");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var cats = JsonSerializer.Deserialize<List<CategoryResponseDto>>(json, JsonOpts)
                               ?? new List<CategoryResponseDto>();
                    ViewBag.Categories = cats;
                    return;
                }
            }
            catch { /* fall through to empty list */ }

            ViewBag.Categories = new List<CategoryResponseDto>();
        }

        // ══════════════════════════════════════════════════════════════════════
        // DASHBOARD
        // ══════════════════════════════════════════════════════════════════════

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.ApiBaseUrl = ApiBaseUrl;
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/course/my-courses");

            List<CourseSummaryDto> courses = new();
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                courses = JsonSerializer.Deserialize<List<CourseSummaryDto>>(content, JsonOpts)
                          ?? new List<CourseSummaryDto>();
            }

            return View(courses);
        }

        // ══════════════════════════════════════════════════════════════════════
        // MY COURSES
        // ══════════════════════════════════════════════════════════════════════

        public async Task<IActionResult> MyCourses()
        {
            ViewBag.ApiBaseUrl = ApiBaseUrl;
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("api/course/my-courses");

            List<CourseSummaryDto> courses = new();
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                courses = JsonSerializer.Deserialize<List<CourseSummaryDto>>(content, JsonOpts)
                          ?? new List<CourseSummaryDto>();
            }

            return View(courses);
        }

        // ══════════════════════════════════════════════════════════════════════
        // SUBMIT FOR REVIEW
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"api/course/{id}/submit-for-review", null);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Course submitted for review!";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Submission failed: {error}";
            }

            return RedirectToAction("MyCourses");
        }

        // ── Unpublish Course ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnpublishCourse(int id)
        {
            var client   = GetAuthenticatedClient();
            var response = await client.PostAsync($"api/course/{id}/unpublish", null);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Course moved back to Draft. It will need to be re-submitted for review.";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Could not unpublish course: {error}";
            }

            return RedirectToAction("MyCourses");
        }

        // ══════════════════════════════════════════════════════════════════════
        // CREATE COURSE
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            await LoadCategoriesAsync();
            return View(new CreateCourseViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(model);
            }

            var client = GetAuthenticatedClient();

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.CourseTitle),       "Title");
            content.Add(new StringContent(model.CourseSubtitle),    "ShortDescription");
            content.Add(new StringContent(model.CourseDescription), "Description");
            content.Add(new StringContent(model.CategoryId!.Value.ToString()), "CategoryId");
            content.Add(new StringContent(model.Level!.Value.ToString()),      "Level");
            content.Add(new StringContent(model.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price");
            content.Add(new StringContent(model.Language),          "Language");

            if (model.CourseImage != null && model.CourseImage.Length > 0)
            {
                var stream = new StreamContent(model.CourseImage.OpenReadStream());
                stream.Headers.ContentType = new MediaTypeHeaderValue(model.CourseImage.ContentType);
                content.Add(stream, "ThumbnailFile", model.CourseImage.FileName);
            }

            var response = await client.PostAsync("api/course", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course created successfully!";
                return RedirectToAction("MyCourses");
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Failed to create course: {errorBody}");
            await LoadCategoriesAsync();
            return View(model);
        }

        // ══════════════════════════════════════════════════════════════════════
        // EDIT COURSE
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"api/course/my-courses/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Course not found or you do not have access.";
                return RedirectToAction("MyCourses");
            }

            var json = await response.Content.ReadAsStringAsync();
            var course = JsonSerializer.Deserialize<CourseResponseDto>(json, JsonOpts);

            if (course is null)
                return RedirectToAction("MyCourses");

            var model = new EditCourseViewModel
            {
                Id                  = course.Id,
                CourseTitle         = course.Title,
                CourseSubtitle      = course.ShortDescription,
                CourseDescription   = course.Description,
                CategoryId          = course.CategoryId,
                Level               = (int)course.Level,
                Price               = course.Price,
                Language            = course.Language,
                ExistingThumbnailUrl = string.IsNullOrEmpty(course.ThumbnailUrl)
                                        ? null
                                        : $"{ApiBaseUrl}{course.ThumbnailUrl}"
            };

            await LoadCategoriesAsync();
            ViewBag.ApiBaseUrl = ApiBaseUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, EditCourseViewModel model)
        {
            // CourseImage is optional during edit, so remove its validation requirement
            ModelState.Remove("CourseImage");

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(model);
            }

            var client = GetAuthenticatedClient();

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.CourseTitle),       "Title");
            content.Add(new StringContent(model.CourseSubtitle),    "ShortDescription");
            content.Add(new StringContent(model.CourseDescription), "Description");
            content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
            content.Add(new StringContent(model.Level.ToString()),  "Level");
            content.Add(new StringContent(model.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price");
            content.Add(new StringContent(model.Language),          "Language");

            if (model.CourseImage != null && model.CourseImage.Length > 0)
            {
                var stream = new StreamContent(model.CourseImage.OpenReadStream());
                stream.Headers.ContentType = new MediaTypeHeaderValue(model.CourseImage.ContentType);
                content.Add(stream, "ThumbnailFile", model.CourseImage.FileName);
            }

            var response = await client.PutAsync($"api/course/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Course updated successfully!";
                return RedirectToAction("MyCourses");
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Failed to update course: {errorBody}");
            await LoadCategoriesAsync();
            return View(model);
        }

        // ══════════════════════════════════════════════════════════════════════
        // DELETE COURSE
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"api/course/{id}");

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Course deleted successfully.";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Could not delete course: {error}";
            }

            return RedirectToAction("MyCourses");
        }

        // ══════════════════════════════════════════════════════════════════════
        // MANAGE CONTENT / CURRICULUM BUILDER
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> ManageContent(int id)
        {
            var client = GetAuthenticatedClient();
            
            // 1. Fetch course details
            var courseResponse = await client.GetAsync($"api/course/my-courses/{id}");
            if (!courseResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Course not found or access denied.";
                return RedirectToAction("MyCourses");
            }
            var courseJson = await courseResponse.Content.ReadAsStringAsync();
            var course = JsonSerializer.Deserialize<CourseResponseDto>(courseJson, JsonOpts);

            // 2. Fetch sections and lessons
            var sectionsResponse = await client.GetAsync($"api/coursesection/course/{id}");
            List<SectionResponseDto> sections = new();
            if (sectionsResponse.IsSuccessStatusCode)
            {
                var sectionsJson = await sectionsResponse.Content.ReadAsStringAsync();
                sections = JsonSerializer.Deserialize<List<SectionResponseDto>>(sectionsJson, JsonOpts) 
                           ?? new List<SectionResponseDto>();
            }

            var model = new ManageContentViewModel
            {
                Course = course ?? new CourseResponseDto(),
                Sections = sections.OrderBy(s => s.DisplayOrder).ToList()
            };

            ViewBag.ApiBaseUrl = ApiBaseUrl;
            return View(model);
        }

        // ── Add Section ──────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(int courseId, string title, int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Section title is required.";
                return RedirectToAction("ManageContent", new { id = courseId });
            }

            var client = GetAuthenticatedClient();
            var dto = new { CourseId = courseId, Title = title.Trim(), DisplayOrder = displayOrder > 0 ? displayOrder : 1 };
            
            var response = await client.PostAsJsonAsync("api/coursesection", dto);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Section added successfully!";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to add section: {error}";
            }

            return RedirectToAction("ManageContent", new { id = courseId });
        }

        // ── Delete Section ───────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSection(int id, int courseId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"api/coursesection/{id}");

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Section deleted successfully!";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to delete section: {error}";
            }

            return RedirectToAction("ManageContent", new { id = courseId });
        }

        // ── Edit Section ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSection(int id, int courseId, string title, int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Section title is required.";
                return RedirectToAction("ManageContent", new { id = courseId });
            }

            var client  = GetAuthenticatedClient();
            var dto     = new { Title = title.Trim(), DisplayOrder = displayOrder > 0 ? displayOrder : 1 };
            var json    = new StringContent(System.Text.Json.JsonSerializer.Serialize(dto),
                              System.Text.Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/coursesection/{id}", json);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Section updated successfully!";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to update section: {error}";
            }

            return RedirectToAction("ManageContent", new { id = courseId });
        }

        // ── Add Lesson ───────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson(int courseId, int sectionId, string title, int contentType, string? videoUrl, string? textContent, int durationInMinutes, int displayOrder, bool isPreview)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Lesson title is required.";
                return RedirectToAction("ManageContent", new { id = courseId });
            }

            var client = GetAuthenticatedClient();
            var dto = new
            {
                SectionId = sectionId,
                Title = title.Trim(),
                ContentType = contentType,
                VideoUrl = videoUrl?.Trim(),
                TextContent = textContent?.Trim(),
                DurationInMinutes = durationInMinutes,
                DisplayOrder = displayOrder > 0 ? displayOrder : 1,
                IsPreview = isPreview
            };

            var response = await client.PostAsJsonAsync("api/lesson", dto);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Lesson added successfully!";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to add lesson: {error}";
            }

            return RedirectToAction("ManageContent", new { id = courseId });
        }

        // ── Delete Lesson ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int id, int courseId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"api/lesson/{id}");

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Lesson deleted successfully!";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to delete lesson: {error}";
            }

            return RedirectToAction("ManageContent", new { id = courseId });
        }

        // ── Edit Lesson ──────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(int id, int courseId, string title,
            int contentType, string? videoUrl, string? textContent,
            int durationInMinutes, int displayOrder, bool isPreview)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Lesson title is required.";
                return RedirectToAction("ManageContent", new { id = courseId });
            }

            var client = GetAuthenticatedClient();
            var dto = new
            {
                Title             = title.Trim(),
                ContentType       = contentType,
                VideoUrl          = videoUrl?.Trim(),
                TextContent       = textContent?.Trim(),
                DurationInMinutes = durationInMinutes,
                DisplayOrder      = displayOrder > 0 ? displayOrder : 1,
                IsPreview         = isPreview
            };
            var json     = new StringContent(System.Text.Json.JsonSerializer.Serialize(dto),
                               System.Text.Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/lesson/{id}", json);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Lesson updated successfully!";
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to update lesson: {error}";
            }

            return RedirectToAction("ManageContent", new { id = courseId });
        }
    }
}
