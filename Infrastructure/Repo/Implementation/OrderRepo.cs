using Domain.Entity;
using Infrastructure.Persistance.DbContext;
using Infrastructure.Repo.Contracts;
using Infrastructure.Repos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repo.Implementation
{
    public class OrderRepo : Repo<Order>, IOrder
    {
        public OrderRepo(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}
