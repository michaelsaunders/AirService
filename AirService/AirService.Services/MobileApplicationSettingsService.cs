using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;
namespace AirService.Services
{
    public class MobileApplicationSettingsService : SimpleService<MobileApplicationSettings>, IMobileApplicationSettingsService
    {
        private readonly IRepository<Venue> _venueRepository;

        public MobileApplicationSettingsService(IRepository<MobileApplicationSettings> mobileApplicationSettingsRepository, IRepository<Venue> venueRepository)
        {
            this.Repository = mobileApplicationSettingsRepository;
            this._venueRepository = venueRepository;
        }

        public MobileApplicationSettings FindByVenue(int venueId)
        {
            return _venueRepository.FindAllIncluding(v => v.MobileApplicationSettings).Where(v => v.Id == venueId).Single().MobileApplicationSettings;
        }

        public static MobileApplicationSettings Create()
        {
            var settings = new MobileApplicationSettings
            {
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                FrostingActive = false,
                DarkTextColour = true,
                DarkButtonColour = true,
                Theme = "#000000",
                VenueAdvertisements = new List<VenueAdvertisement>()
            };

            for (int i = 0; i < 7; i++)
            {
                settings.VenueAdvertisements.Add(new VenueAdvertisement { AdvertisedDay = i, MobileApplicationSettings = settings} );
            }

            return settings;
        }

        MobileApplicationSettings IMobileApplicationSettingsService.GetSettingsAndAdvertisements(int venueId)
        {
            this._venueRepository.SetContextOption(ContextOptions.ProxyCreation,
                                                   false);
            var query = this._venueRepository.Set<Venue>().Include(v => v.MobileApplicationSettings)
                .Include("MobileApplicationSettings.VenueAdvertisements");

            var venue = query.FirstOrDefault(v => v.Id == venueId);
            return venue == null ? null : venue.MobileApplicationSettings;
        }
    }
}