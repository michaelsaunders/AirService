using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class MenuItemOptionService : SimpleService<MenuItemOption>, IMenuItemOptionService
    {
        public MenuItemOptionService(IRepository<MenuItemOption> menuItemOptionRepository)
        {
            this.Repository = menuItemOptionRepository;
        }
    }
}