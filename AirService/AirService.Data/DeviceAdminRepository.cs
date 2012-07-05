using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class DeviceAdminRepository : SimpleRepository<DeviceAdmin>
    {
        public DeviceAdminRepository(IAirServiceContext context) : base(context)
        {
        }
    }
}