using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{ 
    public class VenueAdvertisementRepository : SimpleRepository<VenueAdvertisement>
    {
        public VenueAdvertisementRepository(IAirServiceContext context)
            : base(context)
        { 
        }
    }
}