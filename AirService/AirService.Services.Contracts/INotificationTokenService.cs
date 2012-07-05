using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface INotificationTokenService
    {
        void InsertOrUpdate(NotificationToken token);
    }
}