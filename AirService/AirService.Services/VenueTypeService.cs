using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Services
{
    public class VenueTypeService : SimpleService<VenueType>
    {
        public VenueTypeService(IRepository<VenueType> venueTypeRepository)
        {
            this.Repository = venueTypeRepository;
        }
    }

}