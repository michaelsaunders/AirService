using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface IVenueAccountTypeService
    {
        VenueAccountType GetDefaultAccountType();
    }
}