using AirService.Data.Contracts;

namespace AirService.Services.Notifications
{
    /// <summary>
    ///   Provided to support unit test.
    /// </summary>
    public interface IApnConnectionFactory
    {
        IApnConnection GetApnClientForCustomer(IAirServiceContext dataContext);
        
        IApnConnection GetApnClientForVenue(IAirServiceContext dataContext);
    }
}