using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Threading;
using System.Web;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;
using AirService.Services.Security;
using AirService.Services.Templates;
using AirService.WebServices.Framework;
using AirService.WebServices.Models;
using Ninject;
using Ninject.Extensions.Wcf;

namespace AirService.WebServices
{
    [ServiceContract(Namespace = "urn:airservice:venue")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class VenueWebService : WebServiceBase
    {
        [WebGet(UriTemplate = "lat/{latitude}/lng/{longitude}", ResponseFormat = WebMessageFormat.Json)]
        public List<Venue> GetVenuesByLocation(string latitude,
                                               string longitude)
        {
            return Response(() =>
                                {
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    List<Venue> venues = service.FindVenuesByLocation(double.Parse(latitude),
                                                                                      double.Parse(longitude));
                                    return venues;
                                });
        }

        [WebGet(UriTemplate = "lat/{latitude}/lng/{longitude}/{titleCriteria}", ResponseFormat = WebMessageFormat.Json)]
        public List<Venue> GetVenuesByTitleAndLocation(string latitude,
                                                       string longitude,
                                                       string titleCriteria)
        {
            return Response(() =>
                                {
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    List<Venue> venues = service.FindVenuesByLocation(double.Parse(latitude),
                                                                                      double.Parse(longitude),
                                                                                      titleCriteria: titleCriteria);
                                    return venues;
                                });
        }

        [WebGet(UriTemplate = "lat/{latitude}/lng/{longitude}/radius/{radius}/{titleCriteria}",
            ResponseFormat = WebMessageFormat.Json)
        ]
        public List<Venue> GetVenuesByTitleAndLocationWithin(string latitude,
                                                             string longitude,
                                                             string radius,
                                                             string titleCriteria)
        {
            return Response(() =>
                                {
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    List<Venue> venues = service.FindVenuesByLocation(double.Parse(latitude),
                                                                                      double.Parse(longitude),
                                                                                      double.Parse(radius),
                                                                                      titleCriteria);
                                    return venues;
                                });
        }

        [WebGet(UriTemplate = "lat/{latitude}/lng/{longitude}/radius/{radius}", ResponseFormat = WebMessageFormat.Json)
        ]
        public List<Venue> GetVenuesByLocationWithin(string latitude,
                                                     string longitude,
                                                     string radius)
        {
            return Response(() =>
                                {
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    List<Venue> venues = service.FindVenuesByLocation(double.Parse(latitude),
                                                                                      double.Parse(longitude),
                                                                                      double.Parse(radius));
                                    return venues;
                                });
        }

        [WebGet(UriTemplate = "search/{criteria}", ResponseFormat = WebMessageFormat.Json)]
        public List<Venue> SearchVenues(string criteria)
        {
            return Response(() =>
                                {
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    List<Venue> venues = service.FindVenuesByTitle(criteria);
                                    return venues;
                                });
        }

        [WebGet(UriTemplate = "", ResponseFormat = WebMessageFormat.Json)]
        [BasicAuth(WellKnownSecurityRoles.AllVenueUsers)]
        public Venue GetVenueInfo()
        {
            return Response(() =>
            {
                var identity = this.GetAuthenticatedVenueUser(); 
                var service = KernelContainer.Kernel.Get<IVenueService>();
                return this.CheckAccountType(service.GetVenueById(identity.VenueId));
            });
        }

        [WebGet(UriTemplate = "{venueId}", ResponseFormat = WebMessageFormat.Json)]
        public Venue GetVenueById(string venueId)
        {
            return Response(() =>
                                {
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    return this.CheckAccountType(service.GetVenueById(int.Parse(venueId)));
                                });
        }

        [WebGet(UriTemplate = "{venueId}/menus", ResponseFormat = WebMessageFormat.Json)]
        public List<Menu> GetMenusByVenueId(string venueId)
        {
            return Response(() =>
                                {
                                    var requestedVenueId = int.Parse(venueId);
                                    var service = KernelContainer.Kernel.Get<IMenuService>();
                                    var uuid = HttpContext.Current.Request.Headers["Device-ID"];
                                    List<Menu> menus = service.GetMenus(requestedVenueId, uuid);
                                    return menus;
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.AllVenueUsers)]
        [WebGet(UriTemplate = "menus", ResponseFormat = WebMessageFormat.Json)]
        public List<Menu> GetVenueMenus()
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();
                                    var service = KernelContainer.Kernel.Get<IMenuService>();
                                    List<Menu> menus = service.GetMenus(identity.VenueId, identity.DeviceUuid);
                                    return menus;
                                });
        }

        [WebGet(UriTemplate = "{venueId}/settings", ResponseFormat = WebMessageFormat.Json)]
        public MobileApplicationSettings GetVenueMobileApplicationSettings(string venueId)
        {
            return Response(() =>
                                {
                                    var service = KernelContainer.Kernel.Get<IMobileApplicationSettingsService>();
                                    var settings = service.GetSettingsAndAdvertisements(int.Parse(venueId));
                                    return settings;
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "device/menus", ResponseFormat = WebMessageFormat.Json)]
        public List<Menu> GetDeviceMenus()
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var service = KernelContainer.Kernel.Get<IMenuService>();
                                    List<Menu> menus = service.GetDeviceMenus(identity.VenueId,
                                                                              identity.DeviceId);
                                    return menus;
                                });
        }

        [WebGet(UriTemplate = "{venueId}/menuItem/{menuItemId}/isAvailable", ResponseFormat = WebMessageFormat.Json)]
        public bool IsMenuItemAvailable(string venueId,
                                        string menuItemId)
        {
            return Response(() =>
                                {
                                    var service = KernelContainer.Kernel.Get<IMenuService>();
                                    return service.IsMenuItemAvailable(int.Parse(venueId),
                                                                       int.Parse(menuItemId));
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebInvoke(Method = "POST", UriTemplate = "{venueId}/connect", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult Connect(string venueId)
        {
            return Response(() =>
                                {
                                    string udid = Thread.CurrentPrincipal.Identity.Name;
                                    Customer customer = KernelContainer.Kernel.Get<ICustomerService>().FindByUdid(udid);
                                    customer.ValidateCustomerStatus();
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    VenueConnection connection = service.Connect(int.Parse(venueId),
                                                                                 customer.Id);
                                    int errorCode;
                                    string message;
                                    if (connection.ConnectionStatus == VenueConnection.StatusWaiting)
                                    {
                                        errorCode = 1013;
                                        message = string.Format(Resources.Err1013WaitingToConnectToVenueArgs2,
                                                                connection.Venue.Title,
                                                                connection.VenueId); 
                                    }
                                    else
                                    {
                                        errorCode = 0;
                                        message = "OK";
                                    }

                                    return new OperationResult
                                               {
                                                   IsError = false,
                                                   ErrorCode = errorCode,
                                                   Message = message 
                                               };
                                });
        }
         
        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebGet(UriTemplate = "{venueId}/connectionStatus", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult GetConnectionStatus(string venueId)
        {
            return Response(() =>
            {
                string udid = Thread.CurrentPrincipal.Identity.Name;
                Customer customer = KernelContainer.Kernel.Get<ICustomerService>().FindByUdid(udid);
                customer.ValidateCustomerStatus();
                var service = KernelContainer.Kernel.Get<IVenueService>();
                VenueConnection connection = service.GetVenueConnectionByCustomerId(int.Parse(venueId),
                                                                                    customer.Id);
                int code;
                string message;
                if (connection == null)
                {
                    code = 1026;
                    message = Resources.Err1026CustomerNotConnected;
                }
                else
                {
                    switch (connection.ConnectionStatus)
                    {
                        case VenueConnection.StatusWaiting:
                            code = 1013;
                            message = string.Format(Resources.Err1013WaitingToConnectToVenueArgs2,
                                                    connection.Venue.Title,
                                                    connection.VenueId);
                            break;

                        case SimpleModel.StatusActive:
                        case VenueConnection.StatusClosing:
                            code = 0;
                            message = "Connected";
                            break;

                        default:
                            code = 1026;
                            message = Resources.Err1026CustomerNotConnected;
                            break;
                    }
                }

                return new OperationResult
                           {
                               IsError = false,
                               ErrorCode = code,
                               Message = message
                           };
            });
        }


        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "connection/{customerId}", ResponseFormat = WebMessageFormat.Json)]
        public VenueConnection GetCustomerConnection(string customerId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    VenueConnection connection = service.GetVenueConnectionByCustomerId(identity.VenueId,
                                                                                                        int.Parse(
                                                                                                            customerId));
                                    if (connection != null)
                                    {
                                        return connection;
                                    }

                                    throw new ServiceFaultException(1026,
                                                                    Resources.Err1026CustomerNotConnected);
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "venueAreas", ResponseFormat = WebMessageFormat.Json)]
        public List<VenueArea> GetVenueAreas()
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IVenueAreaService>();
                                    return service.GetVenueAreas(identity.VenueId);
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "connection/accept/{customerId}",
            ResponseFormat = WebMessageFormat.Json)]
        public VenueConnection AcceptConnection(string customerId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    VenueConnection connection = service.AcceptConnection(identity.VenueId,
                                                                                          int.Parse(customerId));
                                    if (connection != null)
                                    {
                                        return connection;
                                    }

                                    throw new ServiceFaultException(1026,
                                                                    Resources.Err1026CustomerNotConnected);
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "customer/{customerId}/locatedAt/{venueAreaId}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult UpdateCustomerLocation(string customerId, string venueAreaId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    service.UpdateCustomerLocation(identity.VenueId,
                                                                   int.Parse(venueAreaId),
                                                                   int.Parse(customerId));
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                });

        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "connection/reject/{customerId}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult RejectConnection(string customerId)
        {
            return this.RejectConnectionWithMessage(customerId,
                                         null);
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "connection/reject/{customerId}/withMessage/{message}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult RejectConnectionWithMessage(string customerId,
                                                           string message)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    service.RejectConnection(identity.VenueId,
                                                             int.Parse(customerId),
                                                             message);
                                    return new OperationResult {Message = "OK"};
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "device/updatecredential", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult UpdateDeviceUdid()
        {
            // if we are here, we already have updated the device UDID
            // See BasicAuthAttribute AuthenticateUser method 
            return new OperationResult
                       {
                           Message = "OK"
                       };
        }
        
        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "device/validatecredential", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult ValidateDeviceCrendential()
        {
            // if we are here, validation was already successful
            return new OperationResult
            {
                Message = "OK"
            };
        }

        [BasicAuth(WellKnownSecurityRoles.VenueAdministratorAndDeviceAdministrators)]
        [WebInvoke(Method = "POST", UriTemplate = "deviceadmin/validatecredential", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult ValidateVenueAdminCrendential()
        {
            // if we are here, validation was already successful
            return new OperationResult
            {
                Message = "OK"
            };
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "connections", ResponseFormat = WebMessageFormat.Json)]
        public List<VenueConnection> GetVenueConnections()
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    return service.GetAllVenueConnections(identity.VenueId);
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "connections/modifiedsince/{fromDate}", ResponseFormat = WebMessageFormat.Json)]
        public List<VenueConnection> GetConnectionsWithModifiedStatesSince(string fromDate)
        {
            return Response(() =>
                                {
                                    DateTime dateTimeSince = DateUtility.FromIso8061FormattedDateString(fromDate);
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    return service.GetModifiedVenueConnectionsSince(identity.VenueId,
                                                                                    dateTimeSince);
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "customer/{customerId}", ResponseFormat = WebMessageFormat.Json)]
        public Customer GetCustomer(string customerId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    return service.GetCustomerFromCurrentVenueConnections(identity.VenueId,
                                                                                          int.Parse(customerId));
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "customer/{customerId}/withOrders", ResponseFormat = WebMessageFormat.Json)]
        public CustomerOrders GetCustomerOrders(string customerId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    var result = new CustomerOrders();
                                    int targetCustomerId = int.Parse(customerId);
                                    result.Customer = service.GetCustomerFromCurrentVenueConnections(identity.VenueId,
                                                                                                     targetCustomerId);
                                    var orderService = KernelContainer.Kernel.Get<IOrderService>();
                                    result.Orders = orderService.GetCustomerCurrentOrders(identity.VenueId,
                                                                                          identity.DeviceId,
                                                                                          targetCustomerId);
                                    return result;
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "customer/{customerId}/freeze/{minutes}",
            ResponseFormat = WebMessageFormat.Json)]
        public VenueConnection FreezeCustomer(string customerId,
                                              string minutes)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    return service.FreezeCustomer(identity.VenueId,
                                                                  int.Parse(customerId),
                                                                  int.Parse(minutes));
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "customer/{customerId}/unfreeze",
            ResponseFormat = WebMessageFormat.Json)]
        public VenueConnection UnfreezeCustomer(string customerId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    return service.UnfreezeCustomer(identity.VenueId,
                                                                    int.Parse(customerId));
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebInvoke(Method = "POST", UriTemplate = "{venueId}/order", BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        public Order PlaceOrder(string venueId,
                                List<OrderItem> orderItems) 
        {
            return this.PlaceOrderAt(venueId,
                                     null,
                                     orderItems);
        }

        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebInvoke(Method = "POST", UriTemplate = "{venueId}/order/at/{orderDate}", BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        public Order PlaceOrderAt(string venueId,
                                  string orderDate,
                                  List<OrderItem> orderItems)
        {
            return Response(() =>
                                {

                                    var identity = this.GetAuthenticatedCustomer();
                                    DateTime timeToOrder = string.IsNullOrWhiteSpace(orderDate)
                                                               ? DateTime.Now
                                                               : DateUtility.FromIso8061FormattedDateString(orderDate); 

                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    Order order = service.PlaceOrder(int.Parse(venueId),
                                                                     identity.Customer.Id,
                                                                     orderItems,
                                                                     timeToOrder);
                                    if (order == null)
                                    {
                                        throw new WebFaultException<OperationResult>(
                                            new OperationResult
                                                {
                                                    ErrorCode = 1004,
                                                    Message = Resources.Err1004UnexpectedError
                                                },
                                            HttpStatusCode.InternalServerError);
                                    }

                                    return order;
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebGet(UriTemplate = "{venueId}/order/{orderId}", ResponseFormat = WebMessageFormat.Json)]
        public Order GetCustomerOrderByOrderId(string venueId,
                                               string orderId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedCustomer(); 
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    return service.GetCustomerOrder(int.Parse(venueId),
                                                                    identity.Customer.Id,
                                                                    int.Parse(orderId));
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebGet(UriTemplate = "{venueId}/orders", ResponseFormat = WebMessageFormat.Json)]
        public List<Order> GetCustomersOrderHistory(string venueId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedCustomer();
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    return service.GetCustomerOrderInCurrentSession(int.Parse(venueId),
                                                                                    identity.Customer.Id);
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "orders", ResponseFormat = WebMessageFormat.Json)]
        public List<Order> GetOrdersForDevice()
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser(); 
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    return service.GetOrdersForDevice(identity.VenueId,
                                                                      identity.DeviceId);
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "orderitems", ResponseFormat = WebMessageFormat.Json)]
        public List<OrderItem> GetOrderItemsForDevice()
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    List<Order> orders = service.GetOrdersForDevice(identity.VenueId,
                                                                                    identity.DeviceId);
                                    return (from order in orders
                                            from item in order.OrderItems
                                            orderby item.UpdateDate descending
                                            select item).ToList();
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "orderitems/modifiedSince/{fromDate}", ResponseFormat = WebMessageFormat.Json)]
        public List<OrderItem> GetModifiedOrderItemsForDevice(string fromDate)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    DateTime dateTimeSince = DateUtility.FromIso8061FormattedDateString(fromDate);
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    List<Order> orders = service.GetModifiedOrdersForDevice(identity.VenueId,
                                                                                            identity.DeviceId,
                                                                                            dateTimeSince);
                                    return (from order in orders
                                            from item in order.OrderItems
                                            orderby item.UpdateDate descending
                                            select item).ToList();
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "orders/modifiedSince/{fromDate}", ResponseFormat = WebMessageFormat.Json)]
        public List<Order> GetModifiedOrdersForDevice(string fromDate)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    DateTime dateTimeSince = DateUtility.FromIso8061FormattedDateString(fromDate);
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    return service.GetModifiedOrdersForDevice(identity.VenueId,
                                                                              identity.DeviceId,
                                                                              dateTimeSince);
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "order/{orderId}", ResponseFormat = WebMessageFormat.Json)]
        public Order GetOrderByIdForDevice(string orderId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    return service.GetOrderForDevice(identity.VenueId,
                                                                     identity.DeviceId,
                                                                     int.Parse(orderId));
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "orderitem/{orderItemId}/confirm",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult ConfirmPendingOrderItem(string orderItemId)
        {
            return this.UpdateOrderItemStatus(int.Parse(orderItemId),
                                              Order.OrderStatusConfirmed,
                                              Resources.APN_OrderConfirmed);
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "orderitem/{orderItemId}/confirmWithMessage/{message}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult ConfirmPendingOrderItemWithMessage(string orderItemId,
                                                                  string message)
        {
            return this.UpdateOrderItemStatus(int.Parse(orderItemId),
                                              Order.OrderStatusConfirmed,
                                              message);
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "orderitem/{orderItemId}/processedWith/{serviceOption}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult UpdateOrderItemAsProcessed(string orderItemId,
                                                          string serviceOption)
        {
            int itemServiceOption = int.Parse(serviceOption);
            string message = itemServiceOption == iPad.ServiceOptionPickup
                                 ? Resources.APN_OrderReadyForPickup
                                 : Resources.APN_OrderReady;
            return this.UpdateOrderItemStatus(int.Parse(orderItemId),
                                              Order.OrderStatusProcessed,
                                              message,
                                              itemServiceOption);
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "orderitem/{orderItemId}/processedWith/{serviceOption}/withMessage/{message}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult UpdateOrderItemAsProcessedWithMessage(string orderItemId,
                                                                     string serviceOption,
                                                                     string message)
        {
            return this.UpdateOrderItemStatus(int.Parse(orderItemId),
                                              Order.OrderStatusProcessed,
                                              message,
                                              int.Parse(serviceOption));
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "orderitem/{orderItemId}/cancel",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult CancelOrderItem(string orderItemId)
        {
            return this.UpdateOrderItemStatus(int.Parse(orderItemId),
                                              Order.OrderStatusCancelled,
                                              null);
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "order/{orderId}/cancel",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult CancelOrder(string orderId)
        {
            return this.CancelOrderWithMessage(orderId,
                                               null);
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "order/{orderId}/cancelWithMessage/{message}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult CancelOrderWithMessage(string orderId,
                                                      string message)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    service.CancelOrder(identity.VenueId,
                                                        int.Parse(orderId),
                                                        message);
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "orderitem/{orderItemId}/cancelWithMessage/{message}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult CancelOrderItemWithMessage(string orderItemId,
                                                          string message)
        {
            return this.UpdateOrderItemStatus(int.Parse(orderItemId),
                                              Order.OrderStatusCancelled,
                                              message);
        }

        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebInvoke(Method = "POST", UriTemplate = "{venueId}/close", BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult RequestToCloseConnection(string venueId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedCustomer();
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    service.RequestToFinalizeAllOrders(int.Parse(venueId),
                                                                       identity.Customer.Id);

                                    return new OperationResult {Message = "OK"};
                                });
        }  

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "customer/{customerId}/finalizeOrders",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult<string> FinalizeCustomerOrders(string customerId)
        {
            return this.FinalizeCustomerOrdersWithMessage(customerId,
                                                          null);
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "receipt/{customerId}", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult<string> GetReceipt(string customerId)
        {
            return Response(() =>
            {
                var identity = this.GetAuthenticatedVenueUser();
                var venueService = KernelContainer.Kernel.Get<IVenueService>();
                var connection = venueService.GetVenueConnectionByCustomerId(identity.VenueId, int.Parse(customerId));
                Receipt receipt = new Receipt();
                receipt.Initialize(identity, connection);
                return new OperationResult<string>
                {
                    Message = "OK",
                    Data = receipt.TransformText()
                };
            });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "customer/{customerId}/finalizeOrdersWithMessage/{message}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult<string> FinalizeCustomerOrdersWithMessage(string customerId,
                                                                         string message)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var orderService = KernelContainer.Kernel.Get<IOrderService>();
                                    var id = orderService.FinializeCustomerOrders(identity.VenueId,
                                                                                  int.Parse(customerId),
                                                                                  message);
                                    var venueService = KernelContainer.Kernel.Get<IVenueService>();
                                    var connection = venueService.GetVenueConnectionById(id);
                                    Receipt receipt = new Receipt();
                                    receipt.Initialize(identity, connection);
                                    return new OperationResult<string>
                                               {
                                                   Message = "OK",
                                                   Data = receipt.TransformText()
                                               };
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueAdministratorAndDeviceAdministrators)]
        [WebInvoke(Method = "POST", UriTemplate = "/reset", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult ResetConnections(string enable)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var service = KernelContainer.Kernel.Get<IVenueService>();
                                    service.ResetConnections(identity.VenueId);
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueAdministratorAndDeviceAdministrators)]
        [WebInvoke(Method = "POST", UriTemplate = "/enable/{enable}", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult EnableAirService(string enable)
        {
            return Response(() =>
            { 
                var identity = this.GetAuthenticatedVenueUser();
                var service = KernelContainer.Kernel.Get<IVenueService>();
                service.EnableService(identity.VenueId,
                                      bool.Parse(enable));
                return new OperationResult
                {
                    Message = "OK"
                };
            });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueAdministratorAndDeviceAdministrators)]
        [WebInvoke(Method = "POST", UriTemplate = "/menu/{menuId}/enable/{enable}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult EnableMenu(string menuId,
                                          string enable)
        {
            return Response(() =>
                                {
                                    var service = KernelContainer.Kernel.Get<IMenuService>();
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    // only Venue admin or device admin could come this far. 
                                    // if it's device admin, ensure the admin has access to associated menu
                                    int menuIdToChange = int.Parse(menuId);
                                    if (!Thread.CurrentPrincipal.IsInRole(WellKnownSecurityRoles.VenueAdministrators))
                                    {
                                        if (!service.CanModifyMenu(identity.Name, menuIdToChange))
                                        {
                                            throw new ServiceFaultException(1033,
                                                                            Resources.
                                                                                Err1033DeviceAdminCannotChangeMenuIfNotAssociatedWithIt);
                                        }
                                    }

                                    service.EnableMenu(identity.VenueId,
                                                       menuIdToChange,
                                                       bool.Parse(enable));
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueAdministratorAndDeviceAdministrators)]
        [WebInvoke(Method = "POST", UriTemplate = "menucategory/{categoryId}/enable/{enable}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult EnableMenuCategory(string categoryId,
                                                  string enable)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();
                                    var service = KernelContainer.Kernel.Get<IMenuService>();
                                    int categoryIdToChange = int.Parse(categoryId);
                                    if (!Thread.CurrentPrincipal.IsInRole(WellKnownSecurityRoles.VenueAdministrators))
                                    {
                                        // means must be device admin
                                        if (!service.CanModifyMenuCategory(identity.Name,
                                                                           categoryIdToChange))
                                        {
                                            throw new ServiceFaultException(1033,
                                                                            Resources.
                                                                                Err1033DeviceAdminCannotChangeMenuIfNotAssociatedWithIt);
                                        }
                                    }

                                    service.EnableMenuCategory(identity.VenueId,
                                                               categoryIdToChange,
                                                               bool.Parse(enable));
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueAdministratorAndDeviceAdministrators)]
        [WebInvoke(Method = "POST", UriTemplate = "menu/{menuId}/printing/{enable}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult EnableMenuPrinting(string menuId,
                                                  string enable)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var service = KernelContainer.Kernel.Get<IMenuService>();
                                    int menuIdToChange = int.Parse(menuId);

                                    if (identity.Device == null)
                                    {
                                        throw new ServiceFaultException(1033,
                                                                        Resources.
                                                                            Err1033DeviceAdminCannotChangeMenuIfNotAssociatedWithIt);
                                    }

                                    if (!Thread.CurrentPrincipal.IsInRole(WellKnownSecurityRoles.VenueAdministrators))
                                    {
                                        if (!service.CanModifyMenu(identity.Name, menuIdToChange))
                                        {
                                            throw new ServiceFaultException(1033,
                                                                            Resources.
                                                                                Err1033DeviceAdminCannotChangeMenuIfNotAssociatedWithIt);
                                        }
                                    }

                                    service.EnableMenuPrinting(identity.VenueId,
                                                               identity.DeviceUuid,
                                                               menuIdToChange,
                                                               bool.Parse(enable));
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueAdministratorAndDeviceAdministrators)]
        [WebInvoke(Method = "POST", UriTemplate = "menuItem/{menuItemId}/printing/{enable}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult EnableMenuItemPrinting(string menuItemId,
                                                      string enable)
        {
            return Response(() =>
            {
                var identity = this.GetAuthenticatedVenueUser();  
                var service = KernelContainer.Kernel.Get<IMenuService>();
                int menuItemIdToChange = int.Parse(menuItemId);
                if (identity.Device == null)
                {
                    throw new ServiceFaultException(1033,
                                                        Resources.
                                                            Err1033DeviceAdminCannotChangeMenuIfNotAssociatedWithIt);
                }

                if (!Thread.CurrentPrincipal.IsInRole(WellKnownSecurityRoles.VenueAdministrators))
                {
                    if (!service.CanModifyMenuItem(identity.Name,
                                                   menuItemIdToChange))
                    {
                        throw new ServiceFaultException(1033,
                                                        Resources.
                                                            Err1033DeviceAdminCannotChangeMenuIfNotAssociatedWithIt);
                    }
                }

                service.EnableMenuItemPrinting(identity.VenueId,
                                               identity.DeviceUuid, 
                                               menuItemIdToChange,
                                               bool.Parse(enable));
                return new OperationResult
                {
                    Message = "OK"
                };
            });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueAdministratorAndDeviceAdministrators)]
        [WebInvoke(Method = "POST", UriTemplate = "menuItem/{menuItemId}/enable/{enable}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult EnableMenuItem(string menuItemId,
                                              string enable)
        {
            return Response(() =>
            {
                var identity = this.GetAuthenticatedVenueUser();
                var service = KernelContainer.Kernel.Get<IMenuService>();
                int menuItemIdToChange = int.Parse(menuItemId);
                if (!Thread.CurrentPrincipal.IsInRole(WellKnownSecurityRoles.VenueAdministrators))
                {
                    if (!service.CanModifyMenuItem(identity.Name,
                                                   menuItemIdToChange))
                    {
                        throw new ServiceFaultException(1033,
                                                        Resources.
                                                            Err1033DeviceAdminCannotChangeMenuIfNotAssociatedWithIt);
                    }
                }

                service.EnableMenuItem(identity.VenueId,
                                       menuItemIdToChange,
                                       bool.Parse(enable));
                return new OperationResult
                {
                    Message = "OK"
                };
            });
        }

        [WebInvoke(Method = "POST", UriTemplate = "deviceAdmin/{deviceAdminEmail}/forgotpassword",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult DeviceAdminForgotPasswords(string deviceAdminEmail)
        {
            return Response(() =>
                                {
                                    var path = ConfigurationManager.AppSettings["EmailTemplatePath"];
                                    var templatePath = Path.Combine(path,
                                                                    "iPadAdmin.htm");
                                    var service = KernelContainer.Kernel.Get<IDeviceAdminService>();
                                    if (!service.SendPasswordEmail(deviceAdminEmail,
                                                                   templatePath))
                                    {
                                        return new OperationResult
                                                   {
                                                       ErrorCode = 1044,
                                                       Message = Resources.Err1044FailedToSendEmail
                                                   };
                                    }

                                    return new OperationResult {Message = "OK"};
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "orderitem/{orderItemId}/undostatus", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult UndoOrderItemStatus(string orderItemId) 
        {
            return this.UndoOrderItemStatusWithMessage(orderItemId,
                                                       null);
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebInvoke(Method = "POST", UriTemplate = "orderitem/{orderItemId}/undostatuswithmessage/{message}",
            ResponseFormat = WebMessageFormat.Json)]
        public OperationResult UndoOrderItemStatusWithMessage(string orderItemId,
                                                              string message)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    service.UndoOrderItemStatus(identity.VenueId,
                                                                identity.DeviceId,
                                                                int.Parse(orderItemId),
                                                                message);
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.Customers)]
        [WebInvoke(Method= "POST", UriTemplate = "orderItem/{orderItemId}/confirmPickup", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult ConfirmOrderItemPickup(string orderItemId)
        {
            // if there is queued notification, instead of sending another notification, we could 
            // send the next notification as a return
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedCustomer(); 
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    var orderItemIdToConfirm = int.Parse(orderItemId);
                                    service.ConfirmOrderItemPickup(identity.Customer.Id,
                                                                   orderItemIdToConfirm);
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                });
        }


        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "device/options", ResponseFormat = WebMessageFormat.Json)]
        public DeviceOptions GetDeviceOptions()
        {
            return Response(() =>{
                                    var ipad = this.GetAuthenticatedVenueUser().Device;  
                                    return new DeviceOptions
                                               {
                                                   IsDeliveryEnabled = ipad.IsDeliveryEnabled,
                                                   IsPickupEnabled = ipad.IsPickupEnabled
                                               };
                                });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueAdministratorAndDeviceAdministrators)]
        [WebInvoke(Method = "POST", UriTemplate = "device/updateOptions", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public OperationResult UpdateDeviceOptions(DeviceOptions options)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    iPad ipad;
                                    var iPadService = KernelContainer.Kernel.Get<IIPadService>();
                                    if (Thread.CurrentPrincipal.IsInRole(WellKnownSecurityRoles.VenueAdministrators))
                                    {
                                        ipad = iPadService.FindByVenueIdAndPin(identity.VenueId,
                                                                               options.Pin);
                                    }
                                    else
                                    {
//must be device admin
                                        var service = KernelContainer.Kernel.Get<IDeviceAdminService>();
                                        ipad =
                                            service.GetAdminDeviceByMatchingPin(Thread.CurrentPrincipal.Identity.Name,
                                                                                options.Pin);
                                    }

                                    if (ipad == null)
                                    {
                                        return new OperationResult
                                                   {
                                                       IsError = true,
                                                       ErrorCode = 1040,
                                                       Message = Resources.Err1040CannotFindDeviceOrAccessDenied
                                                   };
                                    }

                                    ipad.IsDeliveryEnabled = options.IsDeliveryEnabled;
                                    ipad.IsPickupEnabled = options.IsPickupEnabled;
                                    iPadService.Update(ipad); 
                                    return new OperationResult {Message = "OK"};
                                });
        }

        [WebInvoke(Method = "POST", UriTemplate = "registerForNotification", BodyStyle = WebMessageBodyStyle.Bare,
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json)]
        public OperationResult RegisterForNotification(NotificationToken token)
        {
            return Response(() =>
            {
                //TODO: venue specific validation
                var service = KernelContainer.Kernel.Get<INotificationService>();
                service.InsertOrUpdate(token);
                return new OperationResult
                {
                    Message = "OK"
                };
            });
        }

        [BasicAuth(WellKnownSecurityRoles.VenueAdministratorAndDeviceAdministrators)]
        [WebInvoke(Method = "POST", UriTemplate = "broadcastMessage", BodyStyle = WebMessageBodyStyle.Bare,
           RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)] 
        public OperationResult BroadcastMessageToActiveCustomers(SimpleMessage message)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var service = KernelContainer.Kernel.Get<INotificationService>();
                                    service.BroadcastMessageToActiveCustomers(identity.VenueId,
                                                                              message.Message);
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                },
                            use200ForServiceFault: true);
        }

        [BasicAuth(WellKnownSecurityRoles.VenueUsers)]
        [WebGet(UriTemplate = "docket/{orderItemId}", ResponseFormat = WebMessageFormat.Json)]
        public OperationResult<string> GetOrderDocket(string orderItemId)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    var orderItem = service.GetOrderItemForPrinting(identity.VenueId,
                                                                                    identity.DeviceId,
                                                                                    int.Parse(orderItemId));
                                    if (orderItem == null)
                                    {
                                        return new OperationResult<string>
                                                   {
                                                       IsError = false,
                                                       Message = "Printing not required"
                                                   };
                                    }

                                    var docket = new OrderDocket();
                                    docket.Initialize(identity, orderItem, orderItem.Order.VenueConnection.CustomerLocation);
                                    return new OperationResult<string>
                                               {
                                                   Message = "OK",
                                                   Data = docket.TransformText()
                                               };
                                });
        }

        private OperationResult UpdateOrderItemStatus(int orderItemId,
                                                      int newState,
                                                      string message,
                                                      int? serviceOption = null)
        {
            return Response(() =>
                                {
                                    var identity = this.GetAuthenticatedVenueUser();  
                                    var service = KernelContainer.Kernel.Get<IOrderService>();
                                    service.UpdateOrderItemStatus(identity.VenueId,
                                                                  identity.DeviceId,
                                                                  orderItemId,
                                                                  newState,
                                                                  message,
                                                                  serviceOption); 
                                    return new OperationResult
                                               {
                                                   Message = "OK"
                                               };
                                });
        }
         
        private Venue CheckAccountType(Venue venue)
        {
            if ((venue.VenueAccountType & (int)VenueAccountTypes.AccountTypeEvaluation) == (int)VenueAccountTypes.AccountTypeEvaluation)
            {
                return null;
            }

            return venue;
        }
    }
}