using System;
using System.Collections.Generic;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class MenuCategoryDetailViewModel
    {
        public enum ImageType { None = 0, Custom = 1, Stock = 2 };

        public MenuCategory MenuCategory { get; set; }
        public string SelectedStockImage { get; set; }
        public string SelectedCustomImage { get; set; }
        public IEnumerable<String> AvailableStockImages { get; set; }
        public int SelectedImageType { get; set; }
    }
}