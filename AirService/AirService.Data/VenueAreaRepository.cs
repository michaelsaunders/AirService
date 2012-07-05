using System;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{ 
    public class VenueAreaRepository : SimpleRepository<VenueArea>
    {
        public VenueAreaRepository(IAirServiceContext context)
            : base(context)
        { 
        }

        public IQueryable<VenueArea> FindAllByUser(Guid userId)
        {
            var results = from o in Context.Set<VenueArea>()
                          join u in Context.Set<ServiceProvider>().Where(user => user.UserId == userId)
                          on o.Venue.Id equals u.VenueId
                          select o;
            return results;
        }

    }
}