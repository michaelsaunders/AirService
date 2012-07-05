using System;
using AirService.Model;

namespace AirService.Services.Contracts
{
    public interface INotificationService
    {
        void InsertOrUpdate(NotificationToken token);

        void BroadcastMessageToActiveCustomers(int venueId, string message);

        void SendMessageToCustomer(int venueId, int customerId, string message);

        void NotifyVenueDevicesForNewConnectionRequest(int connectionId);

        void NotifyCustomerConnectionAccepted(int connectionId);

        void NotifyCustomerConnectionReject(int connectionId, string message);

        void NotifyVenueDevicesForNewOrder(int venueId, int orderId);

        void NotifyCustomerWithOrderStatus(int orderId, int previousStatus, int newStatus, string message);

        void NotifyCustomerWithOrderItemStatus(int orderItemId, int previousStatus, int newStatus, int? serviceOption, string message);

        void NotifyVenueDevicesForClosingConnection(int connectionId);

        void NotifyCustomerConnectionClosed(int connectionId);

        /// <summary>
        /// Send a queued notification of notificationType among pickup items qeueued 
        /// since fromDate for the given customer at the given venue
        /// </summary>
        void SendQueuedPickupNotification(int venueId, int customerId, DateTime fromDate);
    }
}