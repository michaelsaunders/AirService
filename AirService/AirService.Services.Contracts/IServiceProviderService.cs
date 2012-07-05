using System;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IServiceProviderService : IService<ServiceProvider>
    {
        Venue GetVenueByVenueAdminUserName(Guid userId);
        ServiceProvider FindByVenue(int venueId);
        ServiceProvider FindByUserId(Guid userId);
    }
}