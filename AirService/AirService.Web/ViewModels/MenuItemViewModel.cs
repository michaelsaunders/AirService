using System;
using System.Collections.Generic;
using System.Linq;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class MenuItemViewModel
    {
        public MenuItemViewModel()
        {
        }

        public MenuItemViewModel(MenuItem menuItem)
        {
            this.Id = menuItem.Id;
            this.Title = menuItem.Title;
            this.Description = menuItem.Description;
            this.Price = menuItem.Price;
            this.Status = menuItem.MenuItemStatus? "Active" : "Hidden";
            this.CategoryId = menuItem.MenuCategoryId;
            this.Options = MenuItemOptionViewModel.Wrap(menuItem.MenuItemOptions);
            if (this.Options.Count == 0)
            {
                this.MinPrice = this.Price;
                this.MaxPrice = this.Price;
            }
            else
            {
                this.MinPrice = this.Options.Min(i => i.Price);
                this.MaxPrice = this.Options.Max(i => i.Price); 
            }
        }

        public decimal MinPrice { get; set; }

        public decimal MaxPrice { get; set; }

        public int CategoryId { get; set; }

        public List<MenuItemOptionViewModel> Options { get; set; }

        public string Status { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public string Title { get; set; }

        public int Id { get; set; }

        public static List<MenuItemViewModel> Wrap(IList<MenuItem> menuItems)
        {
            var list = new List<MenuItemViewModel>();
            if (menuItems != null)
            {
                list.AddRange(
                    menuItems.OrderBy(item => item.SortOrder).Select(menuItem => new MenuItemViewModel(menuItem)));
            }

            return list;
        }
    }
}