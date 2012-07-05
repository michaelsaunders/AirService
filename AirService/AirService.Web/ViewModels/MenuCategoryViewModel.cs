using System.Collections.Generic;
using System.Linq;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class MenuCategoryViewModel
    {
        public MenuCategoryViewModel()
        {
        }

        public MenuCategoryViewModel(MenuCategory category)
        {
            this.Id = category.Id;
            this.Title = category.Title;
            this.Status = category.IsLive ? "Active" : "Hidden";
            this.Items = MenuItemViewModel.Wrap(category.MenuItems);
        }

        public string Status { get; set; }

        public string Title { get; set; }

        public List<MenuItemViewModel> Items { get; set; }

        public int Id { get; set; }

        public static List<MenuCategoryViewModel> Wrap(IEnumerable<MenuCategory> categories)
        {
            return categories.OrderBy(c => c.SortOrder).Select(category => new MenuCategoryViewModel(category)).ToList();
        }
    }
}