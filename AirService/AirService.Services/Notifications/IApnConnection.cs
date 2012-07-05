using AirService.Model;

namespace AirService.Services.Notifications
{
    public interface IApnConnection
    {
        void Add(NotificationToken token, byte[] payload);
        
        void SendAsync();

        void Send();
    }
}