using System.Collections.Generic;
using AirService.Model;

namespace AirService.WebTest.ViewModels
{
    public class CustomerViewModel
    {
        public Customer Customer
        {
            get;
            set;
        }
        public IEnumerable<VenueConnection> Connections
        {
            get;
            set;
        }
    }
}