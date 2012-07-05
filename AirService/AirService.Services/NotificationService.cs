using System;
using System.Configuration;
using System.Linq;
using System.Text;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;
using AirService.Services.Notifications;

namespace AirService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IApnConnectionFactory _connectionFactory;
        private readonly IAirServiceContext _context;

        public NotificationService(IAirServiceContext context, IApnConnectionFactory connectionFactory)
        {
            this._context = context;
            this._connectionFactory = connectionFactory;
        }

        #region INotificationService Members

        public void InsertOrUpdate(NotificationToken token)
        {
            if (string.IsNullOrWhiteSpace(token.Udid))
            {
                throw new ServiceFaultException(1001,
                                                Resources.Err1001UdidRequired);
            }

            if (string.IsNullOrWhiteSpace(token.Token))
            {
                throw new ServiceFaultException(1045,
                                                Resources.Err1045RemoteNotificationTokenRequired);
            }

            NotificationToken existingToken =
                this._context.Set<NotificationToken>().Where(t => t.Udid == token.Udid).FirstOrDefault();
            if (existingToken != null)
            {
                existingToken.Token = token.Token;
                existingToken.UpdateDate = DateTime.Now;
            }
            else
            {
                token.CreateDate = DateTime.Now;
                token.UpdateDate = DateTime.Now;
                this._context.Set<NotificationToken>().Add(token);
            }

            this._context.SaveChanges();
        }

        public void BroadcastMessageToActiveCustomers(int venueId, string message)
        {
            int messageLimit = int.Parse(ConfigurationManager.AppSettings["venueBroadcastMessageLimit"]);
            int timeLimit = int.Parse(ConfigurationManager.AppSettings["venueBroadcastMessageTimeLimit"]);
            if (timeLimit > 0)
            {
                DateTime timeLimitStartAt = DateTime.Now.AddMinutes(-timeLimit);
                int numMessagesDuringTimeLimit = (from n in this._context.Notifications
                                                  where n.VenueId == venueId &&
                                                        n.NotificationType == (int) NotificationType.NotifyBroadcast &&
                                                        n.DateTime > timeLimitStartAt
                                                  select n).Count();
                if (numMessagesDuringTimeLimit >= messageLimit)
                {
                    throw new ServiceFaultException(1046,
                                                    string.Format(
                                                        Resources.Err1046BroadcastMessageLimitArg0PerArg1Reached,
                                                        messageLimit,
                                                        timeLimit / 60.0
                                                        ));
                }
            }

            var payload = new SimpleApnPayload
                              {
                                  Message = message,
                                  Data = new DataDictionary
                                             {
                                                 Sender = venueId,
                                                 Code = (int) NotificationType.NotifyBroadcast
                                             }
                              };

            payload.EnableDefaultSound();
            var payloadBytes = payload.ToBytes();
            var notification = new Notification
                                   {
                                       NotificationType = (int) NotificationType.NotifyBroadcast,
                                       DateTime = DateTime.Now,
                                       Message = Encoding.UTF8.GetString(payloadBytes),
                                       VenueId = venueId
                                   };
            this._context.Notifications.Add(notification);
            this._context.SaveChanges();

            var connection = this._connectionFactory.GetApnClientForCustomer(this._context.Clone());
            foreach (NotificationToken token in (from c in this._context.VenueConnections
                                                 join t in this._context.NotificationTokens
                                                     on c.Customer.Udid equals t.Udid
                                                 where c.VenueId == venueId &&
                                                       c.Status == SimpleModel.StatusActive &&
                                                       c.Customer.ReceiveSpecialOffers
                                                 select t))
            {
                connection.Add(token,
                               payloadBytes);
            }

            connection.SendAsync();
        }

        public void SendMessageToCustomer(int venueId, int customerId, string message)
        {
            var token = (from c in this._context.VenueConnections
                         join t in this._context.NotificationTokens
                             on c.Customer.Udid equals t.Udid
                         where c.VenueId == venueId && c.CustomerId == customerId &&
                               c.Status != VenueConnection.Disconnected &&
                               c.Status != VenueConnection.StatusDeleted
                         select t).FirstOrDefault();

            if (token == null)
            {
                return;
            }

            var apnConnection = this._connectionFactory.GetApnClientForCustomer(this._context.Clone());
            var payload = new SimpleApnPayload
                              {
                                  Message = message,
                                  Data = new DataDictionary
                                             {
                                                 Sender = venueId,
                                                 Code = (int) NotificationType.NotifyCustom
                                             }
                              };

            payload.EnableDefaultSound();
            var payloadBytes = payload.ToBytes();
            var notification = new Notification
                                   {
                                       NotificationType = (int) NotificationType.NotifyCustom,
                                       CustomerId = customerId,
                                       VenueId = venueId,
                                       DateTime = DateTime.Now,
                                       Message = Encoding.UTF8.GetString(payloadBytes)
                                   };

            this._context.Notifications.Add(notification);
            this._context.SaveChanges();
            apnConnection.Add(token,
                              payloadBytes);
            apnConnection.SendAsync();
        }

        public void NotifyVenueDevicesForNewConnectionRequest(int connectionId)
        {
            // find all ipads that are registered for push notification
            this._context.SetOption(ContextOptions.LazyLoading,
                                    false);
            var result = (from ipad in this._context.iPads
                          join connection in this._context.VenueConnections on ipad.VenueId equals connection.VenueId
                          join token in this._context.NotificationTokens on ipad.Udid equals token.Udid
                          where connection.Id == connectionId
                          select new
                                     {
                                         connection.VenueId,
                                         token,
                                         connection.Customer,
                                     }).ToList();

            if (result.Count == 0)
            {
                return;
            }

            var firstResult = result.First();
            var payload = new SimpleApnPayload
                              {
                                  Message = string.Format(Resources.APN_NewConnectionRequestFromArg0,
                                                          firstResult.Customer.FirstName),
                                  Data = new DataDictionary
                                             {
                                                 Sender = firstResult.Customer.Id,
                                                 Code = (int) NotificationType.NotifyConnectionRequest
                                             }
                              };

            var payloadBytes = payload.ToBytes();
            Notification notification = new Notification
                                            {
                                                NotificationType = (int) NotificationType.NotifyConnectionRequest,
                                                CustomerId = firstResult.Customer.Id,
                                                VenueId = firstResult.VenueId,
                                                DateTime = DateTime.Now,
                                                Message = Encoding.UTF8.GetString(payloadBytes)
                                            };

            this._context.Notifications.Add(notification);
            this._context.SaveChanges();
            var apnConnection = this._connectionFactory.GetApnClientForVenue(this._context);
            foreach (var dataItem in result)
            {
                apnConnection.Add(dataItem.token,
                                  payloadBytes);
            }

            apnConnection.SendAsync();
        }

        public void NotifyCustomerConnectionAccepted(int connectionId)
        {
            var result = (from customer in this._context.Customers
                          join connection in this._context.VenueConnections on customer.Id equals connection.CustomerId
                          join token in this._context.NotificationTokens on customer.Udid equals token.Udid
                          where connection.Id == connectionId
                          select new
                                     {
                                         connection.VenueId,
                                         connection.CustomerId,
                                         VenueTitle = connection.Venue.Title,
                                         token
                                     }).FirstOrDefault();
            if (result == null)
            {
                Logger.Trace("Customer connection accepted for connection id " + connectionId + " but can't send notification due to missing APN token.");
                return;
            }

            var payload = new SimpleApnPayload
                              {
                                  Message = string.Format(Resources.APN_ConnectedToVenueArg0,
                                                          result.VenueTitle),
                                  Data = new DataDictionary
                                             {
                                                 Sender = result.VenueId,
                                                 Code = (int) NotificationType.NotifyConnectionAccepted
                                             }
                              };

            payload.EnableDefaultSound();
            var payloadBytes = payload.ToBytes();
            Notification notification = new Notification
                                            {
                                                NotificationType = (int) NotificationType.NotifyConnectionAccepted,
                                                CustomerId = result.CustomerId,
                                                VenueId = result.VenueId,
                                                DateTime = DateTime.Now,
                                                Message = Encoding.UTF8.GetString(payloadBytes)
                                            };

            this._context.Notifications.Add(notification);
            this._context.SaveChanges();
            var apnConnection = this._connectionFactory.GetApnClientForCustomer(this._context.Clone());
            apnConnection.Add(result.token,
                              payloadBytes);
            apnConnection.SendAsync();
        }

        public void NotifyCustomerConnectionReject(int connectionId, string message)
        {
            var result = (from c in this._context.VenueConnections
                          join token in this._context.NotificationTokens on c.Customer.Udid equals token.Udid
                          where c.Id == connectionId
                          select new
                                     {
                                         c.Customer,
                                         c.Venue,
                                         token,
                                     }).FirstOrDefault();
            if (result == null)
            {
                Logger.Trace("customer connection is rejected but can't send notification because APN token is not found for customer with connection id " + connectionId);
                return;
            }

            var payload = new SimpleApnPayload
                              {
                                  Message = message ?? Resources.APN_ConnectionRejectMessage,
                                  Data = new DataDictionary
                                             {
                                                 Sender = result.Venue.Id,
                                                 Code = (int) NotificationType.NotifyConnectionRejected
                                             }
                              };

            payload.EnableDefaultSound();
            var payloadBytes = payload.ToBytes();
            Notification notification = new Notification
                                            {
                                                NotificationType = (int) NotificationType.NotifyConnectionRejected,
                                                CustomerId = result.Customer.Id,
                                                VenueId = result.Venue.Id,
                                                DateTime = DateTime.Now,
                                                Message = Encoding.UTF8.GetString(payloadBytes)
                                            };
            this._context.Notifications.Add(notification);
            this._context.SaveChanges();
            var apnConnection = this._connectionFactory.GetApnClientForCustomer(this._context.Clone());
            apnConnection.Add(result.token,
                              payloadBytes);
            apnConnection.SendAsync();
        }

        public void NotifyVenueDevicesForNewOrder(int venueId, int orderId)
        {
            var ipadTokens = (from ipad in this._context.iPads
                              join token in this._context.NotificationTokens on ipad.Udid equals token.Udid
                              where ipad.VenueId == venueId
                              select token).ToList();
            if (ipadTokens.Count == 0)
            {
                Logger.Trace("No venue device can receive notification: venue Id" + venueId);
                return;
            }

            var order = (from o in this._context.Orders
                         join c in this._context.Customers on o.CustomerId equals c.Id
                         where o.Id == orderId && o.VenueId == venueId
                         select new
                                    {
                                        customer = c,
                                        order = o
                                    }).FirstOrDefault();

            if (order == null)
            {
                return;
            }

            var apnPayload = new OrderApnPayload
                                 {
                                     Message = string.Format(Resources.APN_NewOrderFromCustomerArg0,
                                                             order.customer.FirstName),
                                     Data = new OrderDictionary
                                                {
                                                    Code = (int) NotificationType.NotifyNewOrder,
                                                    Sender = order.customer.Id,
                                                    OrderId = orderId
                                                }
                                 };

            var paydloadBytes = apnPayload.ToBytes();
            var notification = new Notification
                                   {
                                       NotificationType = (int) NotificationType.NotifyNewOrder,
                                       CustomerId = order.customer.Id,
                                       VenueId = venueId,
                                       DateTime = DateTime.Now,
                                       Message = Encoding.UTF8.GetString(paydloadBytes)
                                   };
            this._context.Notifications.Add(notification);
            this._context.SaveChanges();
            var apnConnection = this._connectionFactory.GetApnClientForVenue(this._context);
            foreach (var token in ipadTokens)
            {
                apnConnection.Add(token,
                                  paydloadBytes);
            }

            apnConnection.SendAsync();
        }

        public void NotifyCustomerWithOrderStatus(int orderId, int previousStatus, int newStatus, string message)
        {
            var result = (from order in this._context.Orders
                          join token in this._context.NotificationTokens on order.Customer.Udid equals token.Udid
                          where order.Id == orderId
                          select new
                                     {
                                         order,
                                         token
                                     }).FirstOrDefault();
            if (result == null)
            {
                Logger.Trace("Order status notification wasn't sent because APN token for the customer with order id " + orderId + " not found");
                return;
            }

            var payload = new OrderApnPayload
                              {
                                  Message = message,
                                  Data = new OrderDictionary
                                             {
                                                 Code = (int) NotificationType.NotifyOrderStatusChange,
                                                 Sender = result.order.VenueId,
                                                 OldStatus = previousStatus,
                                                 NewStatus = newStatus
                                             }
                              };

            payload.EnableDefaultSound();
            var payloadBytes = payload.ToBytes();
            var notification = new Notification
                                   {
                                       CustomerId = result.order.CustomerId,
                                       DateTime = DateTime.Now,
                                       Message = Encoding.UTF8.GetString(payloadBytes),
                                       NotificationType = (int) NotificationType.NotifyOrderStatusChange,
                                       VenueId = result.order.VenueId
                                   };

            this._context.Notifications.Add(notification);
            this._context.SaveChanges();
            var apnConnection = this._connectionFactory.GetApnClientForCustomer(this._context.Clone());
            apnConnection.Add(result.token,
                              payloadBytes);
            apnConnection.SendAsync();
        }

        public void NotifyCustomerWithOrderItemStatus(int orderItemId, int previousStatus, int newStatus, int? serviceOption, string message)
        {
            int notificationCode = (int) NotificationType.NotifyOrderItemStatusChange;
            bool queueNotification = false;
            //requirement changed so that we only send notification
            // if item is cancelled or processed with pickup service option 
            if (newStatus != Order.OrderStatusCancelled)
            {
                if (newStatus != Order.OrderStatusProcessed ||
                    !serviceOption.HasValue ||
                    serviceOption != iPad.ServiceOptionPickup)
                {
                    return; // no longer required send any notificaiton.
                }

                // we are not going to send a pick-up notification unless this is the only item 
                // processed with pickup service option. 
                notificationCode = (int) NotificationType.NotifyOrderItemStatusWithPickup;
                queueNotification = true;
            }

            var result = (from order in this._context.Orders
                          from orderItem in order.OrderItems
                          join token in this._context.NotificationTokens on order.Customer.Udid equals token.Udid
                          where orderItem.Id == orderItemId
                          select new
                                     {
                                         order,
                                         orderItem,
                                         token
                                     }).FirstOrDefault();
            if (result == null)
            {
                Logger.Trace("Order status notification wasn't sent because the customer's APN token not found.");
                return;
            }

            var customerId = result.order.CustomerId;
            if (queueNotification)
            {
                // if there are order items which are 
                // 1. processed
                // 2. is to be picked up
                // 3. and not picked up yet 
                // and is not the current order item then we are queueing notification, just do nothing
                int venueId = result.order.VenueId;
                VenueConnection connection = (from c in this._context.Set<VenueConnection>()
                                              where c.VenueId == venueId &&
                                                    c.CustomerId == customerId &&
                                                    c.Status != VenueConnection.Disconnected
                                              orderby c.CreateDate descending
                                              select c).FirstOrDefault(); 
                connection.ValidateVenueConnection(false);
                var dateFrom = connection.CreateDate;
                if (this._context.OrderItems.Any(item => item.Order.CustomerId == customerId &&
                                                         item.Order.VenueId == venueId &&
                                                         item.CreateDate >= dateFrom &&
                                                         item.OrderStatus == Order.OrderStatusProcessed &&
                                                         item.ServiceOption == iPad.ServiceOptionPickup &&
                                                         !item.Delivered &&
                                                         item.Id != orderItemId
                    ))
                {
                    Logger.Trace("Notification is being queued up because previous pickup notification wasn't confirmed by customer (This requirement MUST change)");
                    return;
                }
            }

            var payload = new OrderItemApnPayload
                              {
                                  Message = message,
                                  Data = new OrderItemDictionary
                                             {
                                                 Code = notificationCode,
                                                 Sender = result.order.VenueId,
                                                 NewStatus = newStatus,
                                                 OldStatus = previousStatus,
                                                 OrderId = result.order.Id,
                                                 OrderItemId = result.orderItem.Id
                                             }
                              };

            payload.EnableDefaultSound();
            var payloadBytes = payload.ToBytes();
            var notification = new Notification
                                   {
                                       CustomerId = customerId,
                                       DateTime = DateTime.Now,
                                       Message = Encoding.UTF8.GetString(payloadBytes),
                                       NotificationType = notificationCode,
                                       VenueId = result.order.VenueId
                                   };

            this._context.Notifications.Add(notification);
            this._context.SaveChanges();
            var apnConnection = this._connectionFactory.GetApnClientForCustomer(this._context.Clone());
            apnConnection.Add(result.token,
                              payloadBytes);
            apnConnection.SendAsync();
        }

        public void NotifyVenueDevicesForClosingConnection(int connectionId)
        {
            var result = (from ipad in this._context.iPads
                          join token in this._context.NotificationTokens on ipad.Udid equals token.Udid
                          join connection in this._context.VenueConnections on ipad.VenueId equals
                              connection.VenueId
                          where connection.Id == connectionId
                          select new
                                     {
                                         ipad,
                                         token,
                                         connection.Customer
                                     }).ToList();
            if (result.Count == 0)
            {
                Logger.Trace("Session Closing Request Notification wasn't sent because APN token not found for venue with connection id " + connectionId);
                return;
            }

            var first = result.First();
            var payload = new SimpleApnPayload
                              {
                                  Message = string.Format(Resources.APN_CustomerArg0RequestForClosingSession,
                                                          first.Customer.FirstName),
                                  Data = new DataDictionary
                                             {
                                                 Sender = first.Customer.Id,
                                                 Code = (int) NotificationType.NotifyConnectionClosingRequest
                                             }
                              };

            var payloadBytes = payload.ToBytes();
            Notification notification = new Notification
                                            {
                                                CustomerId = first.Customer.Id,
                                                VenueId = first.ipad.VenueId,
                                                DateTime = DateTime.Now,
                                                Message = Encoding.UTF8.GetString(payloadBytes),
                                                NotificationType = (int) NotificationType.NotifyConnectionClosingRequest
                                            };
            this._context.Notifications.Add(notification);
            this._context.SaveChanges();
            var apnConnectin = this._connectionFactory.GetApnClientForVenue(this._context);
            foreach (var item in result)
            {
                apnConnectin.Add(item.token, payloadBytes);
            }

            apnConnectin.SendAsync();
        }

        public void NotifyCustomerConnectionClosed(int connectionId)
        {
            var result = (from connection in this._context.VenueConnections 
                          join token in  this._context.NotificationTokens on connection.Customer.Udid equals token.Udid
                          where connection.Id == connectionId
                          select new
                          {
                              token,
                              connection.Customer,
                              connection.Venue
                          }).ToList();
            if (result.Count == 0)
            {
                Logger.Trace("Session Closing Notification wasn't sent because APN token not found for customer with connection id " + connectionId);
                return;
            }

            var first = result.First();
            var payload = new SimpleApnPayload
            {
                Message = string.Format(Resources.APN_SessionAtArgs0Closed,
                                        first.Venue.Title),
                Data = new DataDictionary
                {
                    Sender = first.Venue.Id,
                    Code = (int)NotificationType.NotifyConnectionClosed
                }
            };

            payload.EnableDefaultSound();
            var payloadBytes = payload.ToBytes();
            Notification notification = new Notification
            {
                CustomerId = first.Customer.Id,
                VenueId = first.Venue.Id,
                DateTime = DateTime.Now,
                Message = Encoding.UTF8.GetString(payloadBytes),
                NotificationType = (int)NotificationType.NotifyConnectionClosed
            };

            this._context.Notifications.Add(notification);
            this._context.SaveChanges();
            var apnConnectin = this._connectionFactory.GetApnClientForCustomer(this._context.Clone());
            foreach (var item in result)
            {
                apnConnectin.Add(item.token, payloadBytes);
            }

            apnConnectin.SendAsync();
        }

        public void SendQueuedPickupNotification(int venueId, int customerId, DateTime fromDate)
        {
            OrderItem orderItem = (from order in this._context.Orders
                                   from item in order.OrderItems
                                   where order.VenueId == venueId &&
                                         order.CustomerId == customerId &&
                                         order.CreateDate >= fromDate &&
                                         item.OrderStatus == Order.OrderStatusProcessed &&
                                         item.ServiceOption == iPad.ServiceOptionPickup &&
                                         !item.Delivered
                                   orderby item.UpdateDate ascending
                                   select item).FirstOrDefault(); 
            if (orderItem == null)
            {
                Logger.Trace("No more queued order items for pick up notification...");
                return; 
            }

            var token = (from c in this._context.Customers
                         join t in this._context.NotificationTokens on c.Udid equals t.Udid
                         where c.Id == customerId
                         select t).FirstOrDefault();
            if (token == null)
            {
                Logger.Trace("Unexpectedly can't find customer APN token to send out queued pickup notification...");
                return;// not registered for notification
            }

            Logger.Trace("Sending out next queued notification..."); 
            var notificationCode = (int) NotificationType.NotifyOrderItemStatusWithPickup;
            var payload = new OrderItemApnPayload
                              {
                                  Message = Resources.APN_OrderReadyForPickup,
                                  Data = new OrderItemDictionary
                                             {
                                                 Code = notificationCode,
                                                 Sender = venueId,
                                                 NewStatus = Order.OrderStatusProcessed,
                                                 OldStatus = Order.OrderStatusConfirmed,
                                                 OrderId = orderItem.OrderId,
                                                 OrderItemId = orderItem.Id
                                             }
                              };

            payload.EnableDefaultSound();
            var payloadBytes = payload.ToBytes();
            var notification = new Notification
                                   {
                                       CustomerId = customerId,
                                       DateTime = DateTime.Now,
                                       Message = Encoding.UTF8.GetString(payloadBytes),
                                       NotificationType = notificationCode,
                                       VenueId = venueId
                                   };

            this._context.Notifications.Add(notification);
            this._context.SaveChanges();
            var apnConnection = this._connectionFactory.GetApnClientForCustomer(this._context.Clone());
            apnConnection.Add(token,
                              payloadBytes);
            apnConnection.SendAsync(); 
        }

        #endregion
    }
}