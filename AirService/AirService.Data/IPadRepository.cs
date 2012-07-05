using System;
using System.Data.Entity;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{ 
    public class IPadRepository : SimpleRepository<iPad>
    {
        public IPadRepository(IAirServiceContext context)
            : base(context)
        { 
        }

        public IQueryable<iPad> FindAllByUser(Guid userId)
        {
            return FindAllByUserIncluding(userId, true);
        }

        public IQueryable<iPad> FindAllByUser(Guid userId, bool activeOnly)
        {
            return FindAllByUserIncluding(userId, activeOnly);
        }

        public IQueryable<iPad> FindAllByUserIncluding(Guid userId, bool activeOnly, params System.Linq.Expressions.Expression<Func<iPad, object>>[] includeProperties)
        {
            var results = from o in Context.Set<iPad>()
                          join u in Context.Set<ServiceProvider>().Where(user => user.UserId == userId)
                          on o.VenueId equals u.VenueId
                          select o;
            if (activeOnly)
            {
                results = results.Where(i => i.Status == SimpleModel.StatusActive);
            }
            
            foreach (var includeProperty in includeProperties)
            {
                results = results.Include(includeProperty);
            }
            return results;
        }

    }
}