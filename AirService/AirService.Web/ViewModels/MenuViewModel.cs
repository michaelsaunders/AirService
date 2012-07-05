using System;
using System.Collections.Generic;
using System.Linq;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class MenuViewModel
    {
        public MenuViewModel(Menu menu)
        {
            this.Id = menu.Id;
            this.Title = menu.Title;
            this.DisplayTitle = menu.DisplayTitle;
            this.OrderSentTo = menu.AssignedDevices == null ? null : string.Join(", ", menu.AssignedDevices.Where(r=>r.iPad.Status == SimpleModel.StatusActive).Select(relation => relation.iPad.Name));
            this.Schedule = menu.Is24Hour ? "24 hours" : @String.Format("{0:g} - {1:g}", menu.ShowFrom, menu.ShowTo);
            this.Status = menu.MenuStatus ? "Active" : "Hidden";
        }

        public string DisplayTitle { get; set; }

        public string Status { get; set; }

        public string Schedule { get; set; }

        public string OrderSentTo { get; set; }

        public string Title { get; set; }

        public int Id { get; set; }

        public static List<MenuViewModel> Wrap(IEnumerable<Menu> menus)
        {
            return menus.OrderBy(menu => menu.SortOrder).Select(menu => new MenuViewModel(menu)).ToList();
        }
    }
}