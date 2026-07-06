using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Identity
{
    public interface IRoleService
    {
        Task<Result> CreateRoleAsync(string roleName);
        List<string> GetAllRoles();
    }
}
