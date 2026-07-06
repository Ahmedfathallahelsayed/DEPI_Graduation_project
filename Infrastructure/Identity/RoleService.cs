using Application.Common;
using Application.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Identity
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public RoleService(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
        }

        public async Task<Result> CreateRoleAsync(string roleName)
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));

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

        public List<string> GetAllRoles()
        {

            return roleManager.Roles.Select(r => r.Name).ToList();
        }
    }
}
