using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public IList<VenueArea> VenueAreas { get; set; }
        public IList<Customer> Customers { get; set; }
    }
}