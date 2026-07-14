using Domain.Entity;
using Infrastructure.Repos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repo.Contracts
{
    public interface ICourse : IRepo<Course>
    {
        Task<List<Course>> GetByInstructorAsync(string instructorId);
        Task<Course?> GetByIdWithDetailsAsync(int id);
        Task<List<Course>> GetPublishedAsync(string? search = null, int? categoryId = null);
        Task<List<Course>> GetSubmittedForApprovalAsync();
    }
}
