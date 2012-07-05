using System.Collections.Generic;
using System.Web.Security;
using AirService.Model;

namespace AirService.Web.Areas.Admin.Models
{
    public class VenueDetailViewModel
    {
        public Venue Venue { get; set; }

        public bool HasLockedUsers { get; set; }

        public int ConnectedCustomers { get; set; }

        public List<MembershipUser> VenueAdmins { get; set; }
    }
}
