using System.Collections.Generic;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class IPadViewModel
    {
        public iPad iPad { get; set; }
        public IList<Menu> AvailableMenus { get; set; }
        public int[] SelectedMenus { get; set; }
        public IList<VenueArea> AvailableVenueAreas { get; set; }
    }
}