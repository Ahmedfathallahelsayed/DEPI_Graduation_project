using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Identity
{
    public interface IUserService
    {
        Task<Result<string>> CreateUserAsync(string email, string password);
        Task<Result<string>> CheckLoginCredentialsAsync(string email, string password);
        Task<List<string>> GetUserRolesAsync(string email);
        Task<Result> AssignUserToRole(string appUserId,string roleName);
    }
}
