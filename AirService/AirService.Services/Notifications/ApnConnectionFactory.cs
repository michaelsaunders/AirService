using AirService.Data.Contracts;

namespace AirService.Services.Notifications
{
    /// <summary>
    ///   Concrete class that only resolve at non-unit testing class.
    ///   This is to ensure all services participated in unit tests are not depending 
    ///   on real APN connections
    /// </summary>
    public class ApnConnectionFactory : IApnConnectionFactory
    {
        #region IApnConnectionFactory Members

        IApnConnection IApnConnectionFactory.GetApnClientForVenue(IAirServiceContext dataContext)
        {
            return ApnConnection.GetApnClient(false,
                                              dataContext);
        }

        IApnConnection IApnConnectionFactory.GetApnClientForCustomer(IAirServiceContext dataContext)
        {
            return ApnConnection.GetApnClient(true,
                                              dataContext);
        }

        #endregion
    }
}