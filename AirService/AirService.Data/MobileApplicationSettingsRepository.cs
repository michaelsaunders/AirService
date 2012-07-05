using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class MobileApplicationSettingsRepository : SimpleRepository<MobileApplicationSettings>
    {
        public MobileApplicationSettingsRepository(IAirServiceContext context)
            : base(context)
        {
        }
    }
}
