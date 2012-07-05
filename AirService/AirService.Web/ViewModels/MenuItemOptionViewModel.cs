using System.Collections.Generic;
using System.Linq;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class MenuItemOptionViewModel
    {
        public MenuItemOptionViewModel(MenuItemOption option)
        {
            this.Id = option.Id;
            this.Title = option.Title;
            this.Price = option.Price;
        }

        public MenuItemOptionViewModel()
        {
        }

        public decimal Price { get; set; }

        public string Title { get; set; }

        public int Id { get; set; }

        public static List<MenuItemOptionViewModel> Wrap(IList<MenuItemOption> menuItemOptions)
        {
            var list = new List<MenuItemOptionViewModel>();
            if (menuItemOptions != null)
            {
                var activeOptions = menuItemOptions.Where(option => option.Status == SimpleModel.StatusActive).Select(option => new MenuItemOptionViewModel(option));
                list.AddRange(activeOptions);
            }

            return list;
        }
    }
}