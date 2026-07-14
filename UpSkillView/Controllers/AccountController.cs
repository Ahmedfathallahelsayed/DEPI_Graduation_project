using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using UpSkillView.Models.Auth;

namespace UpSkillView.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login(string tab = "login", string role = "Student")
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToDashboard();
            }
            
            ViewData["ActiveTab"] = tab;
            ViewData["Role"] = role;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("UpSkillAPI");
            var content = new StringContent(JsonSerializer.Serialize(new { email = model.Email, password = model.Password }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/account/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                
                // The API returns the token directly as a string or quoted string
                var token = responseString.Trim('"');

                await SignInUser(token, model.RememberMe);

                return RedirectToDashboard();
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", new LoginViewModel());
            }

            var client = _httpClientFactory.CreateClient("UpSkillAPI");
            
            // Map our RegisterViewModel to what the API expects (RegisterReq)
            var requestPayload = new
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.RegisterEmail,
                Password = model.RegisterPassword,
                MobileNumber = model.PhoneNumber
            };

            var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (model.RoleOptions == "Instructor")
            {
                response = await client.PostAsync("api/account/RegisterAsInstructor", content);
            }
            else
            {
                response = await client.PostAsync("api/account/RegisterAsStudent", content);
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Login", new { tab = "login" });
            }

            var errorString = await response.Content.ReadAsStringAsync();
            
            // Try to parse the error string to show a clean message
            string cleanError = "An unknown error occurred.";
            try
            {
                using var jsonDoc = JsonDocument.Parse(errorString);
                if (jsonDoc.RootElement.TryGetProperty("errors", out var errorsElement))
                {
                    // It's a ValidationProblemDetails object
                    var errorMessages = new List<string>();
                    foreach (var prop in errorsElement.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var err in prop.Value.EnumerateArray())
                            {
                                errorMessages.Add(err.GetString());
                            }
                        }
                    }
                    cleanError = string.Join(" ", errorMessages);
                }
                else if (jsonDoc.RootElement.TryGetProperty("title", out var titleElement))
                {
                    cleanError = titleElement.GetString();
                }
                else
                {
                    cleanError = errorString; // Fallback to raw string if it's valid JSON but unknown format
                }
            }
            catch
            {
                // Not valid JSON, probably a plain string
                cleanError = errorString.Trim('"');
            }

            ModelState.AddModelError(string.Empty, $"Registration failed: {cleanError}");
            return View("Login", new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("JwtToken");
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(string token, bool rememberMe)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var claims = new List<Claim>();
            claims.AddRange(jwt.Claims);
            claims.Add(new Claim("JwtToken", token)); // Store token in claims for auth validation

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
            
            // Store token in session to use in HttpClients later
            HttpContext.Session.SetString("JwtToken", token);
        }

        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Dashboard", "Student");
            }
            else if (User.IsInRole("Instructor"))
            {
                return RedirectToAction("Dashboard", "Instructor");
            }
            else if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            
            return RedirectToAction("Index", "Home");
        }
    }
}
