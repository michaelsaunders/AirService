using System.Collections.Generic;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class VenueAdvertisementViewModel
    {
        public IEnumerable<VenueAdvertisement> VenueAdvertisements { get; set; }
        public string SelectedImageMonday { get; set; }
        public string SelectedImageTuesday { get; set; }
        public string SelectedImageWednesday { get; set; }
        public string SelectedImageThursday { get; set; }
        public string SelectedImageFriday { get; set; }
        public string SelectedImageSaturday { get; set; }
        public string SelectedImageSunday { get; set; }

    }
}