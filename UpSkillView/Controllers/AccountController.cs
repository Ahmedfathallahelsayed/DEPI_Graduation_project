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
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToDashboard();
            }
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
                var result = JsonSerializer.Deserialize<JsonElement>(responseString);
                var token = result.GetProperty("token").GetString();

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
                return View("Login", new LoginViewModel()); // Re-render login view with errors (login view contains both)
            }

            var client = _httpClientFactory.CreateClient("UpSkillAPI");
            
            var requestData = new
            {
                email = model.RegisterEmail,
                password = model.RegisterPassword,
                fullName = model.FullName,
                country = model.Country,
                phoneNumber = model.PhoneNumber
            };

            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            string endpoint = model.RoleOptions == "Instructor" ? "api/account/register/instructor" : "api/account/register/student";
            
            var response = await client.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                // Auto login after register
                var loginContent = new StringContent(JsonSerializer.Serialize(new { email = model.RegisterEmail, password = model.RegisterPassword }), Encoding.UTF8, "application/json");
                var loginResponse = await client.PostAsync("api/account/login", loginContent);
                
                if (loginResponse.IsSuccessStatusCode)
                {
                    var responseString = await loginResponse.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseString);
                    var token = result.GetProperty("token").GetString();

                    await SignInUser(token, false);
                    return RedirectToDashboard();
                }
                
                return RedirectToAction("Login");
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, "Registration failed: " + errorResponse);
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
