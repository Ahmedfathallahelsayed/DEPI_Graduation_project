using Application.Account.Interface;
using Application.Courses.Interfaces;
using Application.Identity;
using Application.JWT;
using Infrastructure.Persistance;
using Infrastructure.Persistance.DbContext;
using Infrastructure.Repo.Contracts;
using Infrastructure.Repo.Implementation;
using Infrastructure.Service.Account;
using Infrastructure.Service.Courses;
using Infrastructure.Service.FileService;
using Infrastructure.Services.Identity;
using Infrastructure.Services.JWT;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            // Add JWT Authentication
            var jwtSettings = configuration.GetSection("Jwt");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["Key"])
                    ),

                    RoleClaimType = ClaimTypes.Role
                };
            });

            // Add DbContext Connection
            services.AddDbContext<AppDBContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MostafaCon")));

            // Registering Repos contracts
            services.AddScoped<ICategory, CategoryRepo>();
            services.AddScoped<ICertificate, CertificateRepo>();
            services.AddScoped<ICourse, CourseRepo>();
            services.AddScoped<ICourseSection, CourseSectionRepo>();
            services.AddScoped<IEnrollment, EnrollmentRepo>();
            services.AddScoped<ILesson, LessonRepo>();
            services.AddScoped<ILessonProgress, LessonProgressRepo>();
            services.AddScoped<IOrder, OrderRepo>();
            services.AddScoped<IOrderItem, OrderItemRepo>();

            // Registering Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            // Adding Identity
            services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDBContext>()
                    .AddDefaultTokenProviders();

            // Register File Saving
            services.AddScoped<IFileService, FileService>();

            // Registering Services in DI
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();

            // Account Service
            services.AddScoped<IAccountService, AccountService>();

            // Course Module Services (Team Member 2)
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICourseService, CourseService>();




            return services;
        }
    }
}
