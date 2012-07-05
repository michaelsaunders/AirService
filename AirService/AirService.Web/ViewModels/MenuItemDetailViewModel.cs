using System;
using System.Collections.Generic;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class MenuItemDetailViewModel
    {
        public enum ImageType { None = 0, Custom = 1, Stock = 2 };

        public MenuItem MenuItem { get; set; }
        public string SelectedStockImage { get; set; }
        public string SelectedCustomImage { get; set; }
        public IEnumerable<String> AvailableStockImages { get; set; }
        public int SelectedImageType { get; set; }
        public string MenuItemOptions { get; set; }
    }
}