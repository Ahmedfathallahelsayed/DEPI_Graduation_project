using Infrastructure.Persistance.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repos.Common
{
    public class Repo<T> : IRepo<T> where T : class
    {
        protected readonly AppDBContext dbContext;

        public Repo(AppDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<T> Create(T entity)
        {
            await dbContext.Set<T>().AddAsync(entity);
            return entity;
        }

        public async Task<T> Delete(int DBId)
        {
            var entity = await dbContext.Set<T>().FindAsync(DBId);
            if (entity == null)
            {
                return null;
            }
            dbContext.Set<T>().Remove(entity);
            return entity;
        }


        public async Task<List<T>> getAll()
        {
            return await dbContext.Set<T>().ToListAsync();
        }

        public async Task<T> getById(int DBId)
        {
            return await dbContext.Set<T>().FindAsync(DBId);
        }

        public Task<T> Update(T entity)
        {
            var model = dbContext.Set<T>().Attach(entity);
            if (model != null)
            {
                model.State = EntityState.Modified;
            }
            return Task.FromResult(entity);
        }
    }
}
