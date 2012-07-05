namespace AirService.Model
{
    public enum NotificationType
    {
        NotifyConnectionRequest = 1001,
        NotifyConnectionClosingRequest = 1002,
        NotifyNewOrder = 1003,
        NotifyConnectionAccepted = 2001,
        NotifyConnectionRejected = 2002,
        NotifyConnectionClosed = 2003,
        NotifyOrderStatusChange = 2004,
        NotifyOrderItemStatusChange = 2005,
        NotifyOrderItemStatusWithDelivery = 2006, 
        NotifyOrderItemStatusWithPickup = 2007,
        NotifyCustom = 2101,
        NotifyBroadcast = 2102
    }
}