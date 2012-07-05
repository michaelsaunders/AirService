using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class ServiceProviderRepository : SimpleRepository<ServiceProvider>
    {
        public ServiceProviderRepository(IAirServiceContext context)
            : base(context)
        { 
        }
    }
}
