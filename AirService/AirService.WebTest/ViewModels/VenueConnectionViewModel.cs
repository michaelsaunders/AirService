using AirService.Model;

namespace AirService.WebTest.ViewModels
{
    public class VenueConnectionViewModel
    {
        public VenueConnection Connection
        {
            get;
            set;
        }

        public Customer Customer
        {
            get;
            set;
        }
        public int TotalOrders
        {
            get;
            set;
        }
        public Venue Venue
        {
            get;
            set;
        }
    }
}