using System;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class VenueRepository : SimpleRepository<Venue>
    {
        public VenueRepository(IAirServiceContext context)
            : base(context)
        { 
        }

        public IQueryable<Venue> FindAllByUser(Guid userId)
        {
            var results = from o in Context.Set<Venue>()
                          join u in Context.Set<ServiceProvider>().Where(user => user.UserId == userId)
                          on o.Id equals u.VenueId
                          select o;
            return results;
        }

    }
}