using System.Collections.Generic;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class VenueAdvertisementService : SimpleService<VenueAdvertisement>
    {
        private readonly MobileApplicationSettingsService _mobileApplicationSettingsService;

        public VenueAdvertisementService(IRepository<VenueAdvertisement> venueAdvertisementRepository, MobileApplicationSettingsService mobileApplicationSettingsService)
        {
            this.Repository = venueAdvertisementRepository;
            this._mobileApplicationSettingsService = mobileApplicationSettingsService;
        }

        public IEnumerable<VenueAdvertisement> FindAllByVenue(int venueId)
        {
            return _mobileApplicationSettingsService.FindByVenue(venueId).VenueAdvertisements;
        }

    }
}