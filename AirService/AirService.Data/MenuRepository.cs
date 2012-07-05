using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class MenuRepository : SimpleRepository<Menu>
    {
        public MenuRepository(IAirServiceContext context)
            : base(context)
        {
        }

        public IQueryable<Menu> FindAllByUser(Guid userId)
        {
            return this.FindAllByUserIncluding(userId, true);
        }

        public IQueryable<Menu> FindAllByUserIncluding(Guid userId, params Expression<Func<Menu, object>>[] includeProperties)
        {
            return FindAllByUserIncluding(userId, true, includeProperties);
        }

        public IQueryable<Menu> FindAllByUserIncluding(Guid userId, bool activeOnly, params Expression<Func<Menu, object>>[] includeProperties)
        {
            var results = from o in this.Context.Set<Menu>()
                          join u in this.Context.Set<ServiceProvider>().Where(user => user.UserId == userId)
                              on o.VenueId equals u.VenueId
                          select o;
            if (activeOnly)
            {
                results = results.Where(m => m.Status == SimpleModel.StatusActive);
            }

            foreach (var includeProperty in includeProperties)
            {
                results = results.Include(includeProperty);
            }
            return results;
        }
    }
}