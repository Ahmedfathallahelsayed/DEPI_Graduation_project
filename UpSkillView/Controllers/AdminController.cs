using Application.Admin.DTOs;
using Application.Courses.DTOs.Course;
using Application.Courses.DTOs.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

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

        private async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return $"An unknown error occurred on the server (Status Code: {response.StatusCode}).";
            }

            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("errors", out var errorsProp))
                {
                    var firstError = errorsProp.EnumerateObject().FirstOrDefault();
                    if (firstError.Value.ValueKind == JsonValueKind.Array)
                    {
                        var messages = new List<string>();
                        foreach (var err in firstError.Value.EnumerateArray())
                        {
                            messages.Add(err.GetString());
                        }
                        return string.Join(" ", messages);
                    }
                    return firstError.Value.ToString();
                }

                if (root.TryGetProperty("error", out var errorProp))
                {
                    return errorProp.GetString() ?? content;
                }
                
                if (root.ValueKind == JsonValueKind.String)
                {
                    return root.GetString() ?? content;
                }
            }
            catch
            {
                // Fallback if not valid JSON
            }

            return content.Trim('"');
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/Admin/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var stats = JsonSerializer.Deserialize<DashboardStatsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return View(stats);
                }
                TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }

            return View(new DashboardStatsDto());
        }

        public async Task<IActionResult> Courses()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/Admin/pending-courses");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var courses = JsonSerializer.Deserialize<List<CourseResponseDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return View(courses ?? new List<CourseResponseDto>());
                }
                TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }

            return View(new List<CourseResponseDto>());
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"api/Admin/courses/{id}/approve", null);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }
            return RedirectToAction("Courses");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var dto = new RejectCourseDto { Reason = reason };
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/Admin/courses/{id}/reject", content);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }
            return RedirectToAction("Courses");
        }

        public async Task<IActionResult> Users()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/Admin/users");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<UserDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return View(users ?? new List<UserDto>());
                }
                TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }

            return View(new List<UserDto>());
        }

        [HttpPost]
        public async Task<IActionResult> Block(string id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"api/Admin/users/{id}/block", null);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"api/Admin/users/{id}/unblock", null);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(string id, string fullName, string phoneNumber)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var dto = new EditUserDto { FullName = fullName, PhoneNumber = phoneNumber };
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"api/Admin/users/{id}", content);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }
            return RedirectToAction("Users");
        }

        // ── Category Management Actions ───────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/Category/all");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<CategoryResponseDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return View(categories ?? new List<CategoryResponseDto>());
                }
                
                TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }

            return View(new List<CategoryResponseDto>());
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(string name, string description = "")
        {
            try
            {
                var client = GetAuthenticatedClient();
                var dto = new CreateCategoryDto { Name = name, Description = description ?? "" };
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/Category", content);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, string name, string description = "", bool isActive = true)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var dto = new UpdateCategoryDto { Name = name, Description = description ?? "", IsActive = isActive };
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"api/Category/{id}", content);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"api/Category/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }
            return RedirectToAction("Categories");
        }
    }
}
