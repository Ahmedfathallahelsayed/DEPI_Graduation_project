using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UpSkillView
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register AppApi HttpClient by appsettings
            builder.Services.AddHttpClient("DrugFinderAPI", client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["ApiSettings:BaseUrl"]
                    ?? "http://localhost:5080");
            });

            // Register Authentication session

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;

                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = async ctx =>
                        {
                            // Skip validation on login/logout to prevent loops
                            var path = ctx.HttpContext.Request.Path;
                            if (path.StartsWithSegments("/Account/Login") ||
                                path.StartsWithSegments("/Account/Logout"))
                                return;

                            var token = ctx.Principal?.FindFirstValue("JwtToken");

                            if (string.IsNullOrEmpty(token))
                            {
                                ctx.RejectPrincipal();
                                await ctx.HttpContext.SignOutAsync(
                                    CookieAuthenticationDefaults.AuthenticationScheme);
                                return;
                            }

                            try
                            {
                                var handler = new JwtSecurityTokenHandler();

                                if (!handler.CanReadToken(token))
                                {
                                    ctx.RejectPrincipal();
                                    await ctx.HttpContext.SignOutAsync(
                                        CookieAuthenticationDefaults.AuthenticationScheme);
                                    return;
                                }

                                var jwt = handler.ReadJwtToken(token);

                                if (jwt.ValidTo < DateTime.UtcNow)
                                {
                                    ctx.RejectPrincipal();
                                    await ctx.HttpContext.SignOutAsync(
                                        CookieAuthenticationDefaults.AuthenticationScheme);
                                    return;
                                }

                                // Key fix: restore JWT into session from
                                // the cookie claim after every restart
                                var sessionToken = ctx.HttpContext.Session
                                                      .GetString("JwtToken");
                                if (string.IsNullOrEmpty(sessionToken))
                                    ctx.HttpContext.Session.SetString("JwtToken", token);
                            }
                            catch
                            {
                                ctx.RejectPrincipal();
                                await ctx.HttpContext.SignOutAsync(
                                    CookieAuthenticationDefaults.AuthenticationScheme);
                            }
                        }
                    };
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
