using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.JWT
{
    public interface ITokenService
    {
        string GenerateToken(string userId, string email, List<string> roles);
    }
}
