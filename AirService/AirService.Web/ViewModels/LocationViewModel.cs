using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class LocationViewModel
    {
        private readonly Customer _customer;
        private readonly Venue _venue;

        public LocationViewModel(VenueConnection venueConnection)
        {
            this._venue = venueConnection.Venue;
            this._customer = venueConnection.Customer;
        }

        public Venue Venue
        {
            get { return this._venue; }
        }

        public Customer Customer
        {
            get { return this._customer; }
        }
    }
}
