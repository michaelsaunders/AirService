using System.Collections.Generic;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class VenueViewModel
    {
        public int AccountType { get; set; }
     
        public Venue Venue { get; set; }
        
        public IList<State> States { get; set; }
        
        public IList<Country> Countries { get; set; }
        
        public IList<VenueType> AvailableVenueTypes { get; set; }
        
        public int[] SelectedVenueTypes { get; set; } 
    }
}