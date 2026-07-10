using Application.Account.DTOs;
using Application.Account.Interface;
using Application.Common;
using Application.Identity;
using Application.JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service.Account
{
    public class AccountService : IAccountService
    {
        private readonly IUserService userService;
        private readonly ITokenService tokenService;

        public AccountService(IUserService userService, ITokenService tokenService)
        {
            this.userService = userService;
            this.tokenService = tokenService;
        }

        public async Task<Result<string>> Login(LoginReq Request)
        {
            // Verify User Credentials
            var loginResult = await userService.CheckLoginCredentialsAsync(Request.Email, Request.Password);

            if(!loginResult.IsSuccess)
            {
                return Result<string>.Failure(loginResult.Error);
            }

            // Get All User Roles
            var Roles = await userService.GetUserRolesAsync(Request.Email);

            // Generate Token
            var token = tokenService.GenerateToken(loginResult.Value, Request.Email, Roles);

            if (token == null)
            {
                return Result<string>.Failure("Error While Creating the Token");
            }

            return Result<string>.Success(token);
        }

        public async Task<Result> RegisterAsAdmin(RegisterReq Request)
        {
            //Create User
            var createResult = await userService.CreateUserAsync(Request.Email, Request.Password, Request.MobileNumber, $"{Request.FirstName} {Request.LastName}");

            if (!createResult.IsSuccess)
            {
                return Result.Failure(createResult.Error);
            }

            // Assign Role
            var roleResult = await userService.AssignUserToRole(createResult.Value, "Admin");

            if (!roleResult.IsSuccess)
            {
                return Result.Failure(roleResult.Error);
            }

            return Result.Success();
        }

        public async Task<Result> RegisterAsInstructorAsync(RegisterReq Request)
        {
            // Create User
            var createResult = await userService.CreateUserAsync(Request.Email, Request.Password,Request.MobileNumber, $"{Request.FirstName} {Request.LastName}");

            if (!createResult.IsSuccess)
            {
                return Result.Failure(createResult.Error);
            }

            // Assign Role
            var roleResult = await userService.AssignUserToRole(createResult.Value, "Instructor");

            if (!roleResult.IsSuccess)
            {
                return Result.Failure(roleResult.Error);
            }

            return Result.Success();

        }

        public async Task<Result> ReqisterAsStudentAsync(RegisterReq Request)
        {
            //Create User
            var createResult = await userService.CreateUserAsync(Request.Email, Request.Password, Request.MobileNumber, $"{Request.FirstName} {Request.LastName}");

            if (!createResult.IsSuccess)
            {
                return Result.Failure(createResult.Error);
            }

            // Assign Role
            var roleResult = await userService.AssignUserToRole(createResult.Value, "Student");

            if (!roleResult.IsSuccess)
            {
                return Result.Failure(roleResult.Error);
            }

            return Result.Success();
        }
    }
}
