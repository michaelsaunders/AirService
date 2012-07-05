using System.Collections.Generic;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IVenueAreaService : IService<VenueArea>
    {
        List<VenueArea> GetVenueAreas(int venueId);
    }
}