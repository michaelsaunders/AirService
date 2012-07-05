using System.Collections.Generic;
using AirService.Model;

namespace AirService.WebTest.ViewModels
{
    public class DeviceTestViewModel
    {
        public List<VenueConnectionViewModel> Connections
        {
            get;
            set;
        }

        public List<CustomerViewModel> Customers
        {
            get;
            set;
        }

        public List<ServiceProvider> ServiceProviders
        {
            get;
            set;
        }

        public List<DeviceAdmin> DeviceAdmins
        {
            get; 
            set;
        } 
    }
}