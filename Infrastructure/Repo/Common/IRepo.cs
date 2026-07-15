using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repos.Common
{
    public interface IRepo<T> where T : class
    {
        public Task<T> getById(int DBId);
        public Task<List<T>> getAll();
        public Task<T> Create(T entity);
        public Task<T> Update(T entity);
        public Task<T> Delete(int DBId);


    }
}
