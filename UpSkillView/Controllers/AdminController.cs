using Application.Admin.DTOs;
using Application.Courses.DTOs.Category;
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

        // ══════════════════════════════════════════════════════════════
        // ADMIN DASHBOARD
        // ══════════════════════════════════════════════════════════════

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/Admin/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var stats = JsonSerializer.Deserialize<DashboardStatsDto>(content, JsonOpts);
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

        // ══════════════════════════════════════════════════════════════
        // COURSE APPROVAL
        // ══════════════════════════════════════════════════════════════

        public async Task<IActionResult> Courses()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/Admin/pending-courses");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var courses = JsonSerializer.Deserialize<List<CourseResponseDto>>(content, JsonOpts);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"api/Admin/courses/{id}/approve", null);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Course approved successfully.";
                }
                else
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var dto = new RejectCourseDto { Reason = reason };
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"api/Admin/courses/{id}/reject", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Course rejected successfully.";
                }
                else
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var result = await client.PostAsync($"api/Admin/courses/{id}/publish", null);

                if (result.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Course published successfully.";
                else
                    TempData["ErrorMessage"] = await GetErrorMessageAsync(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
            }

            return RedirectToAction("Courses");
        }

        // ══════════════════════════════════════════════════════════════
        // USERS (Member 1)
        // ══════════════════════════════════════════════════════════════

        public async Task<IActionResult> Users()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync("api/Admin/users");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<UserDto>>(content, JsonOpts);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block(string id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"api/Admin/users/{id}/block", null);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "User blocked successfully.";
                }
                else
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unblock(string id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.PostAsync($"api/Admin/users/{id}/unblock", null);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "User unblocked successfully.";
                }
                else
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string fullName, string phoneNumber)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var dto = new EditUserDto { FullName = fullName, PhoneNumber = phoneNumber };
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"api/Admin/users/{id}", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "User updated successfully.";
                }
                else
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

        // ══════════════════════════════════════════════════════════════
        // CATEGORY MANAGEMENT
        // ══════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            try
            {
                var client = GetAuthenticatedClient();
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
                    TempData["ErrorMessage"] = await GetErrorMessageAsync(response);
                }

                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"API Connection Error: {ex.Message}";
                return View(new List<CategoryResponseDto>());
            }
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

            try
            {
                var client = GetAuthenticatedClient();
                var payload = new { Name = name.Trim(), Description = description?.Trim() ?? string.Empty };
                var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/category", json);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Category \"{name}\" created successfully.";
                }
                else
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, string name, string? description, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Category name is required.";
                return RedirectToAction("Categories");
            }

            try
            {
                var client = GetAuthenticatedClient();
                var payload = new { Name = name.Trim(), Description = description?.Trim() ?? string.Empty, IsActive = isActive };
                var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"api/category/{id}", json);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Category \"{name}\" updated successfully.";
                }
                else
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.DeleteAsync($"api/category/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Category deleted successfully.";
                }
                else
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
