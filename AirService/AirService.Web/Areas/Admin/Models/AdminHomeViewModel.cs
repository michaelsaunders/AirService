using System.Data;

namespace AirService.Web.Areas.Admin.Models
{
    public class AdminHomeViewModel
    {
        public DataTable Statistics { get; set; }

        public DataTable Venues { get; set; }

        public bool WebServicesEnabled { get; set; }

        public int VenueListSortColumn { get; set; }

        public bool VenueListSortAscending { get; set; }
    }
}
