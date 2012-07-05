using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class MenuItemOptionRepository : SimpleRepository<MenuItemOption>
    {
        public MenuItemOptionRepository(IAirServiceContext context)
            : base(context)
        {
        }
    }
}