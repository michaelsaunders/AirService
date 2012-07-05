using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class VenueTypeRepository : SimpleRepository<VenueType>
    {
        public VenueTypeRepository(IAirServiceContext context)
            : base(context)
        {
        }
    }

}
