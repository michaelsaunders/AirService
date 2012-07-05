using System;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class MenuCategoryRepository : SimpleRepository<MenuCategory>
    {
        public MenuCategoryRepository(IAirServiceContext context)
            : base(context)
        {
        }

        public IQueryable<MenuCategory> FindAllByUser(Guid userId)
        {
            var results = from o in this.Context.Set<MenuCategory>()
                          join u in this.Context.Set<ServiceProvider>().Where(user => user.UserId == userId)
                              on o.Menu.Venue.Id equals u.VenueId
                          select o;
            return results;
        }

    }
}