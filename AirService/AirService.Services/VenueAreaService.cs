using System;
using System.Collections.Generic;
using System.Linq;
using AirService.Data;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class VenueAreaService : SimpleService<VenueArea>, IVenueAreaService
    {
        public VenueAreaService(IRepository<VenueArea> venueAreaRepository)
        {
            this.Repository = venueAreaRepository;
        }

        public override IQueryable<VenueArea> FindAllByUser(Guid userId)
        {
            return ((VenueAreaRepository)Repository).FindAllByUser(userId);
        }

        public List<VenueArea> GetVenueAreas(int venueId)
        {
            return (from venueArea in this.Repository.FindAll()
                    where venueArea.VenueId == venueId && venueArea.Status == SimpleModel.StatusActive
                    select venueArea).ToList();
        }
    }
}