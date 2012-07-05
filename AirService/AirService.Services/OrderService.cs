using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Contracts;

namespace AirService.Services
{
    public class OrderService : SimpleService<Order>, IOrderService
    {
        private readonly INotificationService _notificationService;

        public OrderService(IRepository<Order> orderRepository,
                            INotificationService notificationService)
        {
            this._notificationService = notificationService;
            this.Repository = orderRepository;
        }

        #region IOrderService Members

        public Order PlaceOrder(int venueId,
                                int customerId,
                                List<OrderItem> orderItems,
                                DateTime orderDate)
        {
            if (orderItems.Count == 0)
            {
                throw new ServiceFaultException(1017,
                                                Resources.Err1017CannotProcessOrderWithEmptyItems);
            }

            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);

            var connections = this.Repository.Set<VenueConnection>()
                .Include(c => c.Venue)
                .Include(c => c.Customer);
            var session = (from connection in connections
                           where
                               connection.VenueId == venueId &&
                               connection.CustomerId == customerId &&
                               connection.Status != VenueConnection.Disconnected
                           select connection).FirstOrDefault();
            session.ValidateVenueConnection();
            if (session.ConnectionStatus != VenueConnection.StatusActive)
            {
                throw new ServiceFaultException(1036,
                                                Resources.Err1036CannotPlaceOrderIfNotActiveCustomer);
            }

            var venue = session.Venue;
            venue.ValidateVenueStatus();
            if (venue.VenueAccountType != (int) VenueAccountTypes.AccountTypeFull)
            {
                throw new ServiceFaultException(1048,
                                                Resources.Err1048CannotConnectToVenueWhichIsNotFullAccountType);
            }

            var menuItemIds = orderItems.Select(item => item.MenuItemId).ToArray();
            var optionIds = orderItems.Select(item => item.MenuItemOptionId).ToArray();
            var set = from menuItem in this.Repository.Set<MenuItem>().Where(item => menuItemIds.Contains(item.Id) &&
                                                                                     item.MenuCategory.Menu.VenueId ==
                                                                                     venueId)
                      select new
                                 {
                                     menu = menuItem.MenuCategory.Menu,
                                     category = menuItem.MenuCategory,
                                     menuItem,
                                     options = from option in menuItem.MenuItemOptions
                                               where optionIds.Contains(option.Id)
                                               select option
                                 };

            var menuItems = set.AsEnumerable().Select(item => item.menuItem).ToList();
            // ******************************************************
            // OK now we got every thing we need to carry on. 
            // Anything happens here should be IN-MEMORY query only.
            // ******************************************************
            // now verfy each order items
            var pairs = (from orderItem in orderItems
                         join menuItem in menuItems on orderItem.MenuItemId equals menuItem.Id into g
                         select new
                                    {
                                        orderItem,
                                        menuItem = g.DefaultIfEmpty().FirstOrDefault()
                                    }).ToArray();

            // select order item which menu item didn't match 
            var errors = new List<OperationResult>();
            foreach (var orderItem in from pair in pairs
                                      where pair.menuItem == null
                                      select pair.orderItem)
            {
                // can't process because the item isn't found
                errors.Add(new OperationResult
                               {
                                   Id = orderItem.MenuItemId,
                                   IsError = true,
                                   ErrorCode = 1009,
                                   Message = string.Format(Resources.Err1009MenuItemArg0NotAvailable,
                                                           orderItem.MenuItemId)
                               });
            }

            if (errors.Count > 0)
            {
                throw new ServiceFaultException(1016,
                                                Resources.Err1016FailedToPlaceOrderDueToOneOrMoreItems,
                                                errors);
            }

            var today = DateTime.Today.ToString("dd/MM/yyyy"); 
            var order = new Order
                            {
                                VenueId = venueId,
                                Venue = venue,
                                CustomerId = customerId,
                                Customer = session.Customer,
                                VenueConnection = session,
                                VenueConnectionId = session.Id,
                                OrderDate = orderDate,
                                CreateDate = DateTime.Now,
                                UpdateDate = DateTime.Now,
                                OrderStatus = Order.OrderStatusPending,
                                Status = SimpleModel.StatusActive,
                                OrderItems = new List<OrderItem>()
                            };

            decimal total = new decimal(0);
            foreach (var pair in pairs)
            {
                MenuItem menuItem = pair.menuItem;
                if (!menuItem.MenuItemStatus || menuItem.Status != SimpleModel.StatusActive)
                {
                    // can't process because the item isn't active
                    errors.Add(new OperationResult
                                   {
                                       Id = menuItem.Id,
                                       IsError = true,
                                       ErrorCode = 1009,
                                       Message = string.Format(Resources.Err1009MenuItemArg0NotAvailable,
                                                               menuItem.Title)
                                   });
                    continue;
                }

                MenuCategory menuCategory = menuItem.MenuCategory;
                if (!menuCategory.IsLive || menuCategory.Status != SimpleModel.StatusActive)
                {
                    // can't process because the item's category isn't active
                    errors.Add(new OperationResult
                                   {
                                       Id = menuItem.Id,
                                       IsError = true,
                                       ErrorCode = 1008,
                                       Message = string.Format(Resources.Err1008MenuCategoryArg0NotAvailable,
                                                               menuCategory.Title)
                                   });
                    continue;
                }

                Menu menu = menuCategory.Menu;
                if (!menu.MenuStatus || menu.Status != SimpleModel.StatusActive)
                {
                    // can't process because the item's menu isn't active
                    errors.Add(new OperationResult
                                   {
                                       Id = menuItem.Id,
                                       IsError = true,
                                       ErrorCode = 1007,
                                       Message = string.Format(
                                           Resources.Err1007MenuArg0NotAvailable,
                                           menu.Title)
                                   });
                }

                // now verify the price.
                var orderItem = pair.orderItem;
                orderItem.Id = 0;
                orderItem.MenuItem = menuItem;
                if (orderItem.Quantity < 1)
                {
                    orderItem.Quantity = 1;
                }

                decimal itemPrice;
                if (orderItem.MenuItemOptionId.HasValue)
                {
                    MenuItemOption menuItemOption =
                        menuItem.MenuItemOptions.FirstOrDefault(option => option.Id == orderItem.MenuItemOptionId);
                    if (menuItemOption == null || menuItemOption.Status != SimpleModel.StatusActive)
                    {
                        errors.Add(new OperationResult
                                       {
                                           Id = menuItem.Id,
                                           IsError = true,
                                           ErrorCode = 1007,
                                           Message = string.Format(
                                               Resources.Err1015MenuItemOptionArg0OfMenuItemArg1NotAvailable,
                                               menuItemOption == null
                                                   ? orderItem.MenuItemOptionId.ToString()
                                                   : menuItemOption.Title,
                                               menuItem.Title)
                                       });
                        continue;
                    }

                    orderItem.MenuItemOption = menuItemOption;
                    orderItem.Name = string.Format("{0}({1})", menuItem.Title, menuItemOption.Title);
                    itemPrice = menuItemOption.Price*orderItem.Quantity;
                }
                else
                {
                    orderItem.Name = menuItem.Title;
                    itemPrice = menuItem.Price*orderItem.Quantity;
                }

                if (itemPrice != orderItem.Price)
                {
                    errors.Add(new OperationResult
                                   {
                                       ErrorCode = 1023,
                                       IsError = true,
                                       Id = menuItem.Id,
                                       Message =
                                           string.Format(Resources.Err1023OrderItemArg0PriceMismatch,
                                                         menuItem.Id)
                                   });
                }

                if (!menu.Is24Hour)
                {
                    var from = DateTime.ParseExact(today + " " + menu.ShowFrom,
                                                   "dd/MM/yyyy HHmm",
                                                   CultureInfo.CurrentCulture);
                    var to = DateTime.ParseExact(today + " " + menu.ShowTo,
                                                 "dd/MM/yyyy HHmm",
                                                 CultureInfo.CurrentCulture);

                    bool canOrder;
                    if (to < from)
                    {
                        canOrder = (orderDate >= from.AddDays(-1) && orderDate <= to) ||
                                   (orderDate >= from && orderDate <= to.AddDays(1));
                    }
                    else
                    {
                        canOrder = orderDate >= from && orderDate <= to;
                    }

                    if (!canOrder)
                    {
                        errors.Add(new OperationResult
                                       {
                                           ErrorCode = 1018,
                                           Id = menuItem.Id,
                                           IsError = true,
                                           Message =
                                               string.Format(Resources.Err1018MenuItemOnlyAvailableBetweenArg0AndArg1,
                                                             menuItem.Title,
                                                             menu.ShowFrom,
                                                             menu.ShowTo
                                               )
                                       });
                        continue;
                    }
                }

                total += orderItem.Price;
                orderItem.CreateDate = DateTime.Now;
                orderItem.UpdateDate = DateTime.Now;
                orderItem.Status = SimpleModel.StatusActive;
                orderItem.OrderTime = orderDate;
                order.OrderItems.Add(orderItem);
                orderItem.Order = order;
                this.Repository.Insert(orderItem);
            }

            if (errors.Count > 0)
            {
                throw new ServiceFaultException(1016,
                                                Resources.Err1016FailedToPlaceOrderDueToOneOrMoreItems,
                                                errors);
            }

            if(session.Orders == null)
            {
                session.Orders = new List<Order>();
            }

            session.Orders.Add(order);
            order.TotalAmount = total;
            this.Repository.Insert(order);
            this.Repository.Save();
            this._notificationService.NotifyVenueDevicesForNewOrder(venueId, order.Id);
            return order;
        }

        public List<Order> GetCustomerOrderInCurrentSession(int venueId, int customerId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            // get current connection created date time
            var session = (from connection in this.Repository.Set<VenueConnection>()
                           where
                               connection.VenueId == venueId &&
                               connection.CustomerId == customerId &&
                               connection.Status != VenueConnection.Disconnected
                           select connection).FirstOrDefault();
            session.ValidateVenueConnection(false);
            var createdDate = session.ConnectedSince;

            return (from order in this.Repository.FindAll()
                    where order.VenueId == venueId &&
                          order.CustomerId == customerId &&
                          order.Status == SimpleModel.StatusActive &&
                          order.OrderStatus != Order.OrderStatusCancelled &&
                          order.CreateDate >= createdDate &&
                          order.OrderItems.Any(orderItem => orderItem.OrderStatus != Order.OrderStatusCancelled)
                    orderby order.CreateDate descending
                    select new
                               {
                                   order,
                                   orderItems = order.OrderItems.Where(orderItem => orderItem.OrderStatus != Order.OrderStatusCancelled), 
                                   menuItems = from orderItem in order.OrderItems
                                               where orderItem.OrderStatus != Order.OrderStatusCancelled
                                               select orderItem.MenuItem,
                                   menuCategoires = from orderItem in order.OrderItems
                                                    where orderItem.OrderStatus != Order.OrderStatusCancelled
                                                    select orderItem.MenuItem.MenuCategory
                               }
                   ).AsEnumerable().Select(item => item.order).ToList();
        }

        //public List<Order> GetCustomerOrders(int venueId, int customerId, int maxRecords)
        //{
        //    this.Repository.SetContextOption(ContextOptions.ProxyCreation,
        //                                     false);
        //    return (from order in this.Repository.FindAll()
        //                .Include(o => o.OrderItems)
        //                .Include("OrderItems.MenuItem")
        //                .Include("OrderItems.MenuItem.MenuCategory")
        //            where order.VenueId == venueId &&
        //                  order.CustomerId == customerId &&
        //                  order.Status == SimpleModel.StatusActive
        //            orderby order.CreateDate descending
        //            select order
        //           ).Take(maxRecords).ToList();
        //}

        /// <summary>
        ///   Get current customer orders for a venue which is current
        /// </summary>
        public List<Order> GetCustomerCurrentOrders(int venueId,
                                                    int iPadId,
                                                    int customerId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var orders = (from order in this.Repository.FindAll()
                          where order.VenueId == venueId &&
                                order.CustomerId == customerId &&
                                order.OrderStatus != Order.OrderStatusFinalized &&
                                order.Status == SimpleModel.StatusActive
                          select new
                                     {
                                         order,
                                         orderItems = order.OrderItems.Where(orderItem => orderItem.OrderStatus != Order.OrderStatusCancelled),
                                         ipads = from orderItem in order.OrderItems
                                                 select orderItem.iPad,
                                         menuItems = from orderItem in order.OrderItems
                                                     where orderItem.OrderStatus != Order.OrderStatusCancelled
                                                     select orderItem.MenuItem,
                                         menuCategory = from orderItem in order.OrderItems
                                                        where orderItem.OrderStatus != Order.OrderStatusCancelled
                                                        select orderItem.MenuItem.MenuCategory
                                     }).AsEnumerable().Select(item => item.order).ToList();

            this.MarkAssignableOrderItemsForiPad(iPadId,
                                                 orders);
            return orders;
        }

        public Order GetCustomerOrder(int venueId, int customerId, int orderId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var result = (from order in this.Repository.FindAll()
                          where order.VenueId == venueId &&
                                order.CustomerId == customerId &&
                                order.Id == orderId &&
                                order.Status == SimpleModel.StatusActive
                          select new
                                     {
                                         order,
                                         orderItems = from orderItem in order.OrderItems
                                                      where orderItem.OrderStatus != Order.OrderStatusCancelled
                                                      select orderItem,
                                         menuItems = from orderItem in order.OrderItems
                                                     where orderItem.OrderStatus != Order.OrderStatusCancelled
                                                     select orderItem.MenuItem,
                                         menuCategories = from orderItem in order.OrderItems
                                                          where orderItem.OrderStatus != Order.OrderStatusCancelled
                                                          select orderItem.MenuItem.MenuCategory,
                                     }).AsEnumerable().Select(item => item.order).FirstOrDefault();
            if (result == null)
            {
                throw new ServiceFaultException(1019,
                                                Resources.Err1019OrderNotFound);
            }

            return result;
        }

        /// <summary>
        ///   Return all active orders or orders that are finalized within the last 24 hours.
        /// </summary>
        public List<Order> GetOrdersForDevice(int venueId, int iPadId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var minCreatedDate = DateTime.Now.AddHours(-24);
            var orders = (from order in this.Repository.FindAll()
                              .Include(o => o.OrderItems)
                              .Include("OrderItems.iPad")
                              .Include("OrderItems.MenuItem")
                              .Include("OrderItems.MenuItem.MenuCategory")
                          where order.VenueId == venueId &&
                                ((order.OrderStatus != Order.OrderStatusFinalized && order.VenueConnection.ConnectionStatus != VenueConnection.Disconnected) ||
                                 (order.OrderStatus == Order.OrderStatusFinalized && order.CreateDate >= minCreatedDate)) &&
                                order.Status == SimpleModel.StatusActive &&
                                order.VenueConnection.ConnectionStatus != VenueConnection.Disconnected
                          select order).ToList();

            this.MarkAssignableOrderItemsForiPad(iPadId,
                                                 orders);
            return orders;
            //this.Repository.SetContextOption(ContextOptions.ProxyCreation,
            //                                 false);
            //var minCreatedDate = DateTime.Now.AddHours(-24);
            //int[] accessibleMenus = (from menu in this.Repository.Set<Menu>()
            //                         where
            //                             menu.VenueId == venueId &&
            //                             menu.IPads.Any(ipad => ipad.Id == iPadId)
            //                         select menu.Id).ToArray();
            //if (accessibleMenus.Length == 0)
            //{
            //    return new List<Order>();
            //}

            //var query = (from order in this.Repository.FindAll()
            //              from orderItem in order.OrderItems
            //              where
            //                  order.VenueId == venueId &&
            //                  ((order.OrderStatus != Order.OrderStatusFinalized) ||
            //                   (order.OrderStatus == Order.OrderStatusFinalized && order.CreateDate >= minCreatedDate)) &&
            //                  order.Status == SimpleModel.StatusActive
            //              select
            //                  new
            //                      {
            //                          order,
            //                          orderItems = from item in order.OrderItems
            //                                       where
            //                                           accessibleMenus.Contains(orderItem.MenuItem.MenuCategory.MenuId)
            //                                       select item,
            //                          iPads = from item in order.OrderItems
            //                                  where
            //                                      accessibleMenus.Contains(orderItem.MenuItem.MenuCategory.MenuId)
            //                                  select item.iPad,
            //                          menuItems = from item in order.OrderItems
            //                                      where accessibleMenus.Contains(item.MenuItem.MenuCategory.MenuId)
            //                                      select orderItem.MenuItem,
            //                          category = from item in order.OrderItems
            //                                     where accessibleMenus.Contains(item.MenuItem.MenuCategory.MenuId)
            //                                     select orderItem.MenuItem.MenuCategory
            //                      }
            //             );



            //var orders = query.AsQueryable().Select(item => item.order).ToList(); 
            //this.MarkAssignableOrderItemsForiPad(iPadId,
            //                                     orders);
            //return orders;
        }

        public List<Order> GetModifiedOrdersForDevice(int venueId, int iPadId, DateTime dateTimeSince)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var minCreatedDate = DateTime.Now.AddHours(-24);
            var orders = (from order in this.Repository.FindAll()
                              .Include(o => o.OrderItems)
                              .Include("OrderItems.iPad")
                              .Include("OrderItems.MenuItem")
                              .Include("OrderItems.MenuItem.MenuCategory")
                          where order.VenueId == venueId &&
                                ((order.OrderStatus != Order.OrderStatusFinalized && order.VenueConnection.ConnectionStatus != VenueConnection.Disconnected) ||
                                 (order.OrderStatus == Order.OrderStatusFinalized && order.CreateDate >= minCreatedDate)) &&
                                order.Status == SimpleModel.StatusActive &&
                                (order.UpdateDate >= dateTimeSince ||
                                 order.OrderItems.Any(item => item.UpdateDate >= dateTimeSince))
                          select order).ToList();
            this.MarkAssignableOrderItemsForiPad(iPadId,
                                                 orders);
            return orders;
        }

        public Order GetOrderForDevice(int venueId, int iPadId, int orderId)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation,
                                             false);
            var result = (from order in this.Repository.FindAll()
                              .Include(o => o.OrderItems)
                              .Include("OrderItems.MenuItem")
                              .Include("OrderItems.MenuItem.MenuCategory")
                              .Include("OrderItems.iPad")
                          where order.Id == orderId &&
                                order.VenueId == venueId &&
                                order.Status == SimpleModel.StatusActive
                          select order
                         ).FirstOrDefault();
            if (result == null)
            {
                throw new ServiceFaultException(1019,
                                                Resources.Err1019OrderNotFound);
            }

            this.MarkAssignableOrderItemsForiPad(iPadId,
                                                 new[]
                                                     {
                                                         result
                                                     });
            return result;
        }

        public void UpdateOrderItemStatus(int venueId,
                                          int iPadId,
                                          int orderItemId,
                                          int newState,
                                          string message,
                                          int? serviceOption = null)
        {
            switch (newState)
            {
                case Order.OrderStatusConfirmed:
                case Order.OrderStatusProcessed:
                case Order.OrderStatusCancelled:
                case Order.OrderStatusPending:
                    break;

                default:
                    throw new InvalidOperationException();
            }

            var result = (from order in this.Repository.FindAll()
                          from orderItem in order.OrderItems
                          where
                              order.VenueId == venueId &&
                              orderItem.Id == orderItemId
                          select new
                                     {
                                         order,
                                         orderItem,
                                         orderItemiPad = orderItem.iPad,
                                         iPad =
                              (
                                  from relation in orderItem.MenuItem.MenuCategory.Menu.AssignedDevices
                                  where relation.iPadId == iPadId
                                  select relation.iPad).FirstOrDefault()
                                     }
                         ).FirstOrDefault();

            if (result == null)
            {
                throw new ServiceFaultException(1020,
                                                string.Format(Resources.Err1020OrderItemArg0NotFound,
                                                              orderItemId));
            }

            var orderItemToUpdate = result.orderItem;
            if (result.iPad == null )
            {
                throw new ServiceFaultException(1021,
                                                string.Format(
                                                    Resources.Err1021TheiPadCannotProcessTheGivenOrderItemArg0,
                                                    orderItemId));
            }

            if(orderItemToUpdate.iPadId != null && orderItemToUpdate.iPadId != iPadId)
            {
                throw new ServiceFaultException(1039,
                                                string.Format(
                                                    Resources.Err1039OrderItemProcessedByAnotheriPadArg1,
                                                    result.orderItemiPad.Name));
            }

            var orderToUpdate = result.order;
            var assignediPad = result.iPad;
            if (orderToUpdate.OrderStatus == Order.OrderStatusCancelled ||
                orderItemToUpdate.OrderStatus == Order.OrderStatusCancelled)
            {
                throw new ServiceFaultException(1025,
                                                Resources.Err1025CannotUpdateCancelledOrderOrCancelledOrderItem);
            }

            if(orderToUpdate.OrderStatus == Order.OrderStatusFinalized)
            {
                throw new ServiceFaultException(1024,
                                                    Resources.Err1024OrderAlreadyProcessed);
            }

            var orderId = orderToUpdate.Id;
            var updateOrderStatus = false;
            if (orderItemToUpdate.OrderStatus == Order.OrderStatusProcessed && newState == Order.OrderStatusConfirmed)
            {
                // undo state
                // to have order object's status as "Processed", all of item status must be "processed" too
                updateOrderStatus = true; // order will be back to "Confirmed" stage
                orderItemToUpdate.ServiceOption = iPad.ServiceOptionNone;
            }
            else if (orderItemToUpdate.OrderStatus == Order.OrderStatusConfirmed && newState == Order.OrderStatusPending)
            {
                // undo state
                if (!(from item in this.Repository.Set<OrderItem>()
                      where
                          item.OrderId == orderId &&
                          item.Id != orderItemId &&
                          (item.OrderStatus == Order.OrderStatusConfirmed ||
                           item.OrderStatus == Order.OrderStatusProcessed)
                      select item).Any())
                {
                    // now order can be set to pending as well
                    updateOrderStatus = true;
                }

                assignediPad = null;
            }
            else
            {
                if (newState == Order.OrderStatusProcessed && newState == orderToUpdate.OrderStatus)
                {
                    throw new ServiceFaultException(1024,
                                                    Resources.Err1024OrderAlreadyProcessed);
                }

                if (newState == Order.OrderStatusConfirmed && orderItemToUpdate.OrderStatus == newState)
                {
                    throw new ServiceFaultException(1022,
                                                    string.Format(
                                                        Resources.Err1022OrderItemArg0IsAlreadyBeingProcessed,
                                                        orderItemId));
                }

                if (serviceOption.HasValue)
                {
                    if ((serviceOption.Value == iPad.ServiceOptionDelivery && assignediPad.IsDeliveryEnabled) ||
                        (serviceOption.Value == iPad.ServiceOptionPickup && assignediPad.IsPickupEnabled))
                    {
                        orderItemToUpdate.ServiceOption = serviceOption.Value;
                    }
                    else
                    {
                        throw new ServiceFaultException(1042,
                                                        Resources.Err1042InvalidOrderItemServiceOption);
                    }
                }

                if (newState != Order.OrderStatusConfirmed)
                {
                    var stat = (from order in this.Repository.FindAll()
                                where order.Id == orderId
                                select new
                                           {
                                               itemCount = (from orderItem in order.OrderItems
                                                            where orderItem.Id != orderItemId &&
                                                                  (newState == Order.OrderStatusCancelled ||
                                                                   orderItem.OrderStatus != Order.OrderStatusCancelled)
                                                            select orderItem).Count(),
                                               itemWithNewStates = (from orderItem in order.OrderItems
                                                                    where orderItem.Id != orderItemId &&
                                                                          orderItem.OrderStatus == newState
                                                                    select orderItem).Count()
                                           }).First();
                    if (stat.itemCount == stat.itemWithNewStates)
                    {
                        updateOrderStatus = true;
                    }
                }
                else if (orderItemToUpdate.Order.OrderStatus != newState)
                {
                    updateOrderStatus = true;
                }
            }

            if (assignediPad == null)
            {
                orderItemToUpdate.iPadId = null; 
                orderItemToUpdate.iPad = null;
            }
            else
            {
                orderItemToUpdate.iPadId = assignediPad.Id;
                orderItemToUpdate.iPad = assignediPad;
            }

            var previousStatus = orderItemToUpdate.OrderStatus;
            orderItemToUpdate.iPad = assignediPad;
            orderItemToUpdate.OrderStatus = newState;
            if(newState == Order.OrderStatusProcessed && serviceOption == iPad.ServiceOptionDelivery)
            {
                orderItemToUpdate.Delivered = true;
            }

            orderItemToUpdate.UpdateDate = DateTime.Now;
            this.Repository.Update(orderItemToUpdate);
            var newTotal = (from order in this.Repository.FindAll()
                            from orderItem in order.OrderItems
                            where
                                order.Id == orderId &&
                                orderItem.OrderStatus != Order.OrderStatusCancelled
                            select orderItem.Price
                           ).Sum();
            
            if (updateOrderStatus || newTotal != orderToUpdate.TotalAmount)
            {
                if(updateOrderStatus)
                {
                    orderToUpdate.OrderStatus = newState;
                }

                orderToUpdate.TotalAmount = newTotal;
                orderToUpdate.UpdateDate = DateTime.Now;
                this.Repository.Update(orderToUpdate);
            }

            this.Repository.Save();

            if (!string.IsNullOrWhiteSpace(message))
            {
                this._notificationService.NotifyCustomerWithOrderItemStatus(orderItemId,
                                                                            previousStatus,
                                                                            newState,
                                                                            serviceOption,
                                                                            message);
            }
        }

        public void CancelOrder(int venueId, int orderId, string message)
        {
            var orderToCancel = (from order in this.Repository.FindAll()
                                 where order.Id == orderId && order.VenueId == venueId
                                 select order).FirstOrDefault();
            
            if (orderToCancel == null)
            {
                throw new ServiceFaultException(1019,
                                                Resources.Err1019OrderNotFound);
            }

            if (orderToCancel.OrderStatus == Order.OrderStatusCancelled)
            {
                throw new ServiceFaultException(1025,
                                                Resources.Err1025CannotUpdateCancelledOrderOrCancelledOrderItem);
            }

            var finishedOrderItems = (from item in orderToCancel.OrderItems
                                      where item.OrderStatus == Order.OrderStatusFinalized ||
                                            item.OrderStatus == Order.OrderStatusProcessed
                                      select item).Count();
            if (orderToCancel.OrderStatus == Order.OrderStatusProcessed ||
                orderToCancel.OrderStatus == Order.OrderStatusFinalized ||
                finishedOrderItems > 0)
            {
                throw new ServiceFaultException(1028,
                                                "Cannot cancel the order because one or more items are processed already. Please cancel individual items that are not processed yet");
            }

            foreach (var orderItem in orderToCancel.OrderItems)
            {
                orderItem.OrderStatus = Order.OrderStatusCancelled;
                orderItem.UpdateDate = DateTime.Now;
                this.Repository.Update(orderItem);
            }

            var previousStatus = orderToCancel.OrderStatus;
            orderToCancel.OrderStatus = Order.OrderStatusCancelled;
            orderToCancel.UpdateDate = DateTime.Now;
            this.Repository.Update(orderToCancel);
            this.Repository.Save();
            if (!string.IsNullOrWhiteSpace(message))
            {
                this._notificationService.NotifyCustomerWithOrderStatus(orderId,
                                                                        previousStatus,
                                                                        Order.OrderStatusCancelled,
                                                                        message);
            }
        }

        public void RequestToFinalizeAllOrders(int venueId, int customerId)
        {
            VenueConnection connection = (from c in this.Repository.Set<VenueConnection>()
                                          where c.VenueId == venueId &&
                                                c.CustomerId == customerId &&
                                                c.Status != VenueConnection.Disconnected
                                          select c).FirstOrDefault();

            connection.ValidateVenueConnection(false);
            if (connection.ConnectionStatus == VenueConnection.StatusClosing)
            {
                return;
            }

            var connectedSince = connection.ConnectedSince;
            var unfinishedOrderItems = (from order in this.Repository.FindAll()
                                        where order.CustomerId == customerId &&
                                              order.VenueId == venueId &&
                                              order.CreateDate >= connectedSince &&
                                              (order.OrderStatus == Order.OrderStatusPending ||
                                               order.OrderStatus == Order.OrderStatusConfirmed)
                                        select order).Count();
            if (unfinishedOrderItems > 0)
            {
                throw new ServiceFaultException(1037,
                                                Resources.Err1037CannotCloseConnectionWhenActiveOrdersExist); 
            }

            connection.Status = VenueConnection.StatusClosing;
            connection.UpdateDate = DateTime.Now;
            Repository.Update(connection);
            Repository.Save();
            this._notificationService.NotifyVenueDevicesForClosingConnection(connection.Id);
        }

        public void UndoOrderItemStatus(int venueId,
                                        int iPadId,
                                        int orderItemId,
                                        string message)
        {
            var orderItem = this.Repository.Set<OrderItem>().Find(orderItemId);
            if (orderItem == null)
            {
                throw new ServiceFaultException(1020,
                                                string.Format(Resources.Err1020OrderItemArg0NotFound,
                                                              orderItemId));
            }

            int newState;
            if (orderItem.OrderStatus == Order.OrderStatusProcessed)
            {
                newState = Order.OrderStatusConfirmed;
            }
            else if (orderItem.OrderStatus == Order.OrderStatusConfirmed)
            {
                newState = Order.OrderStatusPending;
            }
            else
            {
                throw new ServiceFaultException(1038,
                                                Resources.Err1038CannotUndoOrderItemStatus);
            }

            this.UpdateOrderItemStatus(venueId,
                                       iPadId,
                                       orderItemId,
                                       newState,
                                       message);
        }

        public void ConfirmOrderItemPickup(int customerId, int orderItemId)
        {
            Logger.Trace("Order Item picked up by customer " + customerId + " for order item " + orderItemId);
            // Get Current Connection first. 
            this.Repository.SetContextOption(ContextOptions.ProxyCreation, false);
            OrderItem orderItem = (from item in this.Repository.Set<OrderItem>()
                                   where item.Id == orderItemId &&
                                         item.Order.CustomerId == customerId
                                   select new
                                              {
                                                  item,
                                                  item.Order, 
                                                  item.Order.VenueConnection
                                              }).AsEnumerable().Select(set => set.item).FirstOrDefault(); 
            if (orderItem == null)
            {
                throw new ServiceFaultException(1020,
                                                string.Format(Resources.Err1020OrderItemArg0NotFound,
                                                              orderItemId));
            }

            var order = orderItem.Order;
            if (order == null)
            {
                Logger.Trace("Unexpected error. Order object is nil");
                return;
            }

            var connection = order.VenueConnection; 
            connection.ValidateVenueConnection(false);
            //Simply send another pickup notification if exists
            orderItem.Delivered = true;
            this.Repository.Save();
            this._notificationService.SendQueuedPickupNotification(connection.VenueId,
                                                                   customerId,
                                                                   connection.CreateDate); 
        }

        public OrderItem GetOrderItemForPrinting(int venueId, int ipadId, int orderItemId)
        {
            var ids = (from orderItem in this.Repository.Set<OrderItem>()
                       where orderItem.Id == orderItemId
                       select new
                                  {
                                      menuItemId = orderItem.MenuItem.Id,
                                      menuId = orderItem.MenuItem.MenuCategory.MenuId
                                  }).FirstOrDefault();

            var menuOption = this.Repository.Set<DeviceMenuOption>().FirstOrDefault(m => m.MenuId == ids.menuId);
            if (menuOption != null && !menuOption.Print)
            {
                return null;
            }

            var itemOption = this.Repository.Set<DeviceMenuItemOption>().FirstOrDefault(m => m.MenuItemId == ids.menuItemId);
            if (itemOption != null && !itemOption.Print)
            {
                return null;
            }

            this.Repository.SetContextOption(ContextOptions.ProxyCreation, false);
            var orderItems = this.Repository.Set<OrderItem>()
                .Include(v => v.Order)
                .Include("Order.Customer")
                .Include("Order.VenueConnection")
                .Include("Order.VenueConnection.VenueArea");
            return orderItems.FirstOrDefault(item =>
                                             item.Id == orderItemId &&
                                             item.Order.VenueId == venueId);
        }

        public int FinializeCustomerOrders(int venueId, int customerId, string message)
        {
            this.Repository.SetContextOption(ContextOptions.ProxyCreation, true);
            VenueConnection connection = (from c in this.Repository.Set<VenueConnection>()
                                          where c.VenueId == venueId &&
                                                c.CustomerId == customerId &&
                                                c.Status != VenueConnection.Disconnected
                                          select c).FirstOrDefault();

            connection.ValidateVenueConnection(false);
            var sessionId = connection.Id;
            var orders = (from order in this.Repository.FindAll()
                          where order.CustomerId == customerId
                                && order.VenueId == venueId
                                && order.OrderStatus != Order.OrderStatusFinalized &&
                                order.OrderStatus != Order.OrderStatusCancelled &&
                                order.VenueConnectionId == sessionId
                          select order).ToList();

            var finalizedTime = DateTime.Now;
            var itemsNotProcessed = (from order in orders
                                     from orderItem in order.OrderItems
                                     where orderItem.OrderStatus != Order.OrderStatusCancelled &&
                                           orderItem.OrderStatus != Order.OrderStatusProcessed
                                     select orderItem).ToList();
            if(itemsNotProcessed.Count > 0)
            {
                var orderItemIds = string.Join(",",
                                               itemsNotProcessed.Select(item => item.Id).ToArray());
                var errorMessage = string.Format(Resources.Err1028CannotFinalizeOrderBecauseNotAllOrdersAreComplete,
                                                 orderItemIds);
                throw new ServiceFaultException(1028,
                                                errorMessage);
            }

            foreach (var order in orders)
            { 
                foreach (var orderItem in order.OrderItems)
                {
                    orderItem.OrderStatus = Order.OrderStatusFinalized;
                    orderItem.UpdateDate = finalizedTime;
                    this.Repository.Update(orderItem);
                }

                order.OrderStatus = Order.OrderStatusFinalized;
                order.UpdateDate = finalizedTime;
                this.Repository.Update(order);
            }

            connection.UpdateDate = DateTime.Now;
            connection.Status = VenueConnection.Disconnected;
            this.Repository.Update(connection);
            this.Repository.Save();
            this._notificationService.NotifyCustomerConnectionClosed(connection.Id);
            return connection.Id;
        }

        #endregion

        /// <summary>
        ///   Get All order items that are not associated with any iPad yet.
        ///   and update CanAssign flag to true if the iPad is associated with the related menu.
        /// </summary>
        private void MarkAssignableOrderItemsForiPad(int iPadId, IEnumerable<Order> orders)
        {
            var orderIds = orders.Select(order => order.Id).ToList();
            var assignable = (from order in this.Repository.FindAll()
                              from orderItem in order.OrderItems
                              where
                                  orderIds.Contains(order.Id) &&
                                  orderItem.MenuItem.MenuCategory.Menu.AssignedDevices.Any(ipad => ipad.iPadId == iPadId)
                              select new
                                         {
                                             orderId = order.Id,
                                             orderItemId = orderItem.Id,
                                             canAssign = (orderItem.iPad == null || orderItem.iPadId == iPadId)
                                         }).ToList();

            var result = from order in orders
                         from orderItem in order.OrderItems ?? Enumerable.Empty<OrderItem>()
                         join item in assignable on orderItem.Id equals item.orderItemId into g
                         select new
                                    {
                                        orderItem,
                                        info = g.FirstOrDefault()
                                    }; 

            foreach (var item in result)
            {
                if(item.info == null)
                {
                    item.orderItem.Hidden = true;
                    item.orderItem.CanAssign = false;
                }
                else
                {
                    item.orderItem.Hidden = false;
                    item.orderItem.CanAssign = item.info.canAssign;
                }
            }
        }
    }
}