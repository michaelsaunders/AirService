using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IMobileApplicationSettingsService
    {
        MobileApplicationSettings GetSettingsAndAdvertisements(int venueId);
    }
}