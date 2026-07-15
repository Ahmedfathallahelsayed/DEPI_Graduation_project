using Application.Common;
using Application.Identity;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Identity
{
    public class UserService : IUserService
    {
        public UserService(UserManager<AppUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<AppUser> UserManager { get; }

        public async Task<Result> AssignUserToRole(string appUserId,string roleName)
        {
            var user = await UserManager.Users.FirstOrDefaultAsync(u => u.Id == appUserId);

            if (user == null)
            {
                return Result.Failure(roleName + " role assignment failed. User not found.");
            }

            var result = await UserManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                return Result.Success();
            }
            else
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description)); // return all error messages concatenated separated by semicolons
                return Result.Failure(errorMessage);
            }
        }

        public async Task<Result<string>> CheckLoginCredentialsAsync(string email, string password)
        {
            var user = await UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<string>.Failure("Invalid email or password.");
            }
            if (!user.IsActive)
            {
                return Result<string>.Failure("Your account has been blocked by an administrator.");
            }
            var Result = await UserManager.CheckPasswordAsync(user, password);
            if (!Result)
            {
                return Result<string>.Failure("Invalid email or password.");
            }

            return Result<string>.Success(user.Id);
        }

        public async Task<Result<string>> CreateUserAsync(string email, string password, string PhoneNumber, string Fullname)
        {
            var user = new AppUser { UserName = email, Email = email, PhoneNumber = PhoneNumber, FullName = Fullname};
            var result = await UserManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
               return Result<string>.Failure(result.Errors.FirstOrDefault()?.Description ?? "User creation failed.");
            }

            return Result<string>.Success(user.Id);
        }

        public async Task<List<string>> GetUserRolesAsync(string email)
        {
            var user = await UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new List<string>(); // Return an empty list if the user is not found
            }
            var roles = await UserManager.GetRolesAsync(user);

            return roles.ToList();
        }

        public async Task<string> GetUserFullNameAsync(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            return user?.FullName ?? "Unknown";
        }
    }
}
