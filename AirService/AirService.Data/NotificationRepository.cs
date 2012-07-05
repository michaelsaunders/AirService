using AirService.Data.Contracts;
using AirService.Model;

namespace AirService.Data
{
    public class NotificationRepository : SimpleRepository<Notification>
    {
        public NotificationRepository(IAirServiceContext context) : base(context)
        {
        }
    }
}