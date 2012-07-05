using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;

namespace AirService.WebTest.Controllers
{
    public class iPhoneTestController : DeviceTestControllerBase<Customer>
    {
        private readonly IOrderService _orderService;
        private readonly IVenueService _venueService;

        public iPhoneTestController(IRepository<Customer> db,
                                    IOrderService orderService,
                                    IVenueService venueService)
            : base(db)
        {
            this._orderService = orderService;
            this._venueService = venueService;
        }

        [HttpPost]
        public ActionResult AcceptConnection(int venueId,
                                             int customerId)
        {
            this._venueService.AcceptConnection(venueId, customerId);
            if (this.Request.IsAjaxRequest())
            {
                return this.PartialView("_ConnectionList",
                                        this.GetCurrentConnections());
            }

            return this.GetDefaultView();
        }

        [HttpPost]
        public ActionResult RejectConnection(int venueId,
                                             int customerId)
        {
            this._venueService.RejectConnection(venueId,
                                                customerId,
                                                null);
            if (this.Request.IsAjaxRequest())
            {
                return this.PartialView("_ConnectionList",
                                        this.GetCurrentConnections());
            }

            return this.GetDefaultView();
        }


        public ActionResult Index()
        {
            return this.GetDefaultView();
        }

        [HttpPost]
        public ActionResult ConfirmOrderItem(int orderItemId)
        {
            var orderItemWithMenuId = (from item in this.Db.Set<OrderItem>()
                                       where item.Id == orderItemId
                                       select new
                                                  {
                                                      orderItem = item,
                                                      menuId = item.MenuItem.MenuCategory.MenuId
                                                  }).First();

            OrderItem orderItem = orderItemWithMenuId.orderItem;
            string errorMessage = null;
            if (orderItem.OrderStatus == Order.OrderStatusPending)
            {
                var menuId = orderItemWithMenuId.menuId;
                var ipads = (from ipad in this.Db.Set<iPad>()
                             from menu in ipad.AssignedMenus
                             where menu.MenuId == menuId
                             select ipad).ToArray();
                var selectediPad = ipads[Utility.Random.Next(0,
                                                             ipads.Length)];
                try
                {
                    this._orderService.UpdateOrderItemStatus(selectediPad.VenueId,
                                                             selectediPad.Id,
                                                             orderItemId,
                                                             Order.OrderStatusConfirmed, 
                                                             Resources.APN_OrderConfirmed);
                    return this.Json(new Result
                                         {
                                             ipad = selectediPad.Name
                                         });
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                }
            }

            return this.Json(new Result
                                 {
                                     error =
                                         errorMessage ??
                                         "Order item status wasn't 'pending'. Please click refresh button and try again"
                                 });
        }

        [HttpPost]
        public ActionResult ProcessOrderItem(int orderItemId)
        {
            var orderItem = this.Db.Set<OrderItem>().Include(item => item.Order).Include(item => item.iPad)
                .First(item => item.Id == orderItemId);
            string errorMessage = null;
            if (orderItem.OrderStatus == Order.OrderStatusConfirmed)
            {
                try
                {
                    var selectediPad = orderItem.iPad;
                    int serviceOption = orderItem.ServiceOption; 
                    if (serviceOption == iPad.ServiceOptionNone)
                    {
                        if (selectediPad.IsDeliveryEnabled && selectediPad.IsPickupEnabled)
                        {
                            serviceOption = Utility.Random.Next(0,
                                                                2) == 1
                                                ? iPad.ServiceOptionDelivery
                                                : iPad.ServiceOptionPickup;
                        }
                        else if (selectediPad.IsDeliveryEnabled)
                        {
                            serviceOption = iPad.ServiceOptionDelivery;
                        }
                        else if (selectediPad.IsPickupEnabled)
                        {
                            serviceOption = iPad.ServiceOptionPickup;
                        }

                        orderItem.ServiceOption = serviceOption;
                    }
                     
                    var message = orderItem.ServiceOption == iPad.ServiceOptionPickup
                                      ? Resources.APN_OrderReadyForPickup
                                      : Resources.APN_OrderReady;
                    this._orderService.UpdateOrderItemStatus(orderItem.Order.VenueId,
                                                             selectediPad.Id,
                                                             orderItemId,
                                                             Order.OrderStatusProcessed,
                                                             message,
                                                             serviceOption);
                    return this.Json(new Result
                                         {
                                             ipad = selectediPad.Name
                                         });
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                }
            }

            return this.Json(new Result
                                 {
                                     error =
                                         errorMessage ??
                                         "Order item status wasn't 'confirmed'. Please click refresh button and try again"
                                 });
        }

        [HttpPost]
        public ActionResult FinaizeCustomerOrder(int venueId,
                                                 int customerId)
        {
            try
            {
                this._orderService.FinializeCustomerOrders(venueId,
                                                           customerId,
                                                           null);
            }
            catch (Exception e)
            {
                Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                return Content (e.Message);
            }
             
            return this.PartialView("_ConnectionList",
                                    this.GetCurrentConnections());
        }

        public ActionResult CancelOrder(int venueId, int orderId)
        {
            try
            {
                this._orderService.CancelOrder(venueId,
                                               orderId,
                                               "Order #" + orderId + " is cancelled.");
            }
            catch (Exception e)
            {
                this.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                return this.Content(e.Message);
            }

            return this.Content("OK");
        }

        public ActionResult CancelOrderItem(int venueId, int iPadId, int orderItemId)
        {
            try
            {
                this._orderService.UpdateOrderItemStatus(venueId,
                                                         iPadId,
                                                         orderItemId,
                                                         Order.OrderStatusCancelled,
                                                         null);
            }
            catch (Exception e)
            {
                this.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return this.Content(e.Message);
            }

            return this.Content("OK");
        }

        public ActionResult UndoOrderItem(int venueId, int iPadId, int orderItemId)
        {
            try
            {
                this._orderService.UndoOrderItemStatus(venueId,
                                                       iPadId,
                                                       orderItemId,
                                                       "Order Item #" + orderItemId + " status changed.");
            }
            catch (Exception e)
            {
                this.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return this.Content(e.Message);
            }

            return this.Content("OK");
        }

        #region Nested type: Result

        [Serializable]
        public class Result
        {
            public string error
            {
                get;
                set;
            }

            public string html
            {
                get;
                set;
            }

            public string ipad
            {
                get;
                set;
            }
        }

        #endregion
    }
}