using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class VenueConnectionRepository : SimpleRepository<VenueConnection>
    {
        public VenueConnectionRepository(IAirServiceContext context) : base(context)
        {
        }
    }
}