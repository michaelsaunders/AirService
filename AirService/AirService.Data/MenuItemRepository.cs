using System;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{ 
    public class MenuItemRepository : SimpleRepository<MenuItem>
    {
        public MenuItemRepository(IAirServiceContext context)
            : base(context)
        {
        }

        public IQueryable<MenuItem> FindAllByUser(Guid userId)
        {
            var results = from o in Context.Set<MenuItem>()
                    join u in Context.Set<ServiceProvider>().Where(user => user.UserId == userId)
                    on o.MenuCategory.Menu.Venue.Id equals u.VenueId
                    select o;
            return results;
        }
    }
}