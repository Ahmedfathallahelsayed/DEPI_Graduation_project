using Application.Account.DTOs;
using Application.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Account.Interface
{
    public interface IAccountService
    {
        public Task<Result> RegisterAsInstructorAsync(RegisterReq Request);
        public Task<Result> ReqisterAsStudentAsync(RegisterReq Request);
        public Task<Result> RegisterAsAdmin(RegisterReq Request);
        public Task<Result<string>> Login(LoginReq Request);
    }
}
