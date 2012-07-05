using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;
using AirService.Services.Notifications;
using Ninject;

namespace AirService.WebTest.Controllers
{
    public class iPadTestController : DeviceTestControllerBase<Venue>
    {
        private readonly IMenuService _menuService;
        private readonly IOrderService _orderService;
        private readonly IVenueService _venueService;
        
        [Inject]
        public iPadTestController(IRepository<Venue> db,
                                  IMenuService menuService,
                                  IOrderService orderService,
                                  IVenueService venueService)
            : base(db)
        {
            this._menuService = menuService;
            this._orderService = orderService;
            this._venueService = venueService;
        }

        [HttpPost]
        public ActionResult CreateNewCustomer(string firstName,
                                              string lastName)
        {
            var customer = new Customer
                               {
                                   CreateDate = DateTime.Now,
                                   UpdateDate = DateTime.Now,
                                   FacebookId = Utility.Random.NextDouble().ToString(),
                                   FirstName =
                                       string.IsNullOrWhiteSpace(firstName) ? Utility.GetRandomString(5) : firstName,
                                   LastName =
                                       string.IsNullOrWhiteSpace(lastName) ? Utility.GetRandomString(5) : lastName,
                                   Status = SimpleModel.StatusActive,
                                   Image = this.Url.Content("~/content/photo_default.png"),
                                   Udid = Guid.NewGuid().ToString("N").Replace("-",
                                                                               "").PadLeft(40,
                                                                                           '0')
                               };
            this.Db.Set<Customer>().Add(customer);
            this.Db.Save();

            if (this.Request.IsAjaxRequest())
            {
                return
                    this.PartialView("_CustomerList", this.GetCurrentCustomers());
            }

            return this.GetDefaultView();
        }

        public ActionResult Index()
        {
            return this.GetDefaultView();
        }

        [HttpPost]
        public ActionResult PlaceRandomOrder(int venueId,
                                             int customerId)
        {
            VenueConnection connection = (from c in this.Db.Set<VenueConnection>()
                                          where c.CustomerId == customerId && c.VenueId == venueId &&
                                                c.Status != VenueConnection.Disconnected
                                          select c).FirstOrDefault();
            if (connection == null)
            {
                this.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                return new ContentResult
                           {
                               Content = "Connection not found. Please refresh your browser."
                           };
            }

            if (connection.Status == VenueConnection.StatusClosing)
            {
                //for demo purpose
                connection.Status = VenueConnection.StatusActive; 
                this.Db.Save();
            }

            if (!connection.ConnectedSince.HasValue)
            {
                this._venueService.AcceptConnection(venueId, customerId); 
            }

            List<Menu> menus = (from menu in this.Db.Set<Menu>()
                                where menu.VenueId == venueId &&
                                      menu.Status == SimpleModel.StatusActive &&
                                      menu.MenuStatus
                                select menu).ToList();
            if (menus.Count == 0)
            {
                menus = this.CreateRandomMenu(venueId);
            }

            decimal total = 0;
            var newOrder = new Order
                               {
                                   OrderStatus = Order.OrderStatusPending,
                                   OrderDate = DateTime.Now,
                                   CustomerId = customerId,
                                   VenueId = venueId,
                                   VenueConnection =  connection,
                                   VenueConnectionId = connection.Id,
                                   CreateDate = DateTime.Now,
                                   UpdateDate = DateTime.Now,
                                   Status = SimpleModel.StatusActive
                               };

            this.Db.Insert(newOrder);
            Random random = Utility.Random;
            int itemsToOrder = random.Next(1, 10);
            for (int i = 0; i < itemsToOrder; i++)
            {
                int menuId = menus[random.Next(0, menus.Count)].Id;
                List<MenuItem> menuItems = (from category in this.Db.Set<MenuCategory>()
                                            from item in category.MenuItems
                                            where
                                                category.MenuId == menuId &&
                                                category.Status == SimpleModel.StatusActive && category.IsLive
                                                && item.Status == SimpleModel.StatusActive &&
                                                item.MenuItemStatus
                                            select item).ToList();

                MenuItem menuItem = menuItems[random.Next(0, menuItems.Count)];
                List<MenuItemOption> options = menuItem.MenuItemOptions.ToList();
                var quantity = Utility.Random.Next(1, 3);
                var newOrderItem = new OrderItem
                                       {
                                           Quantity = quantity,
                                           MenuItem = menuItem,
                                           MenuItemId = menuItem.Id,
                                           Order = newOrder,
                                           OrderTime = newOrder.CreateDate,
                                           CreateDate = DateTime.Now,
                                           UpdateDate = DateTime.Now,
                                           Status = SimpleModel.StatusActive
                                       };

                if (options.Count > 0 && Utility.Random.Next(0, 2) == 1)
                {
                    MenuItemOption option = options[random.Next(0, options.Count)];
                    newOrderItem.MenuItemOption = option;
                    newOrderItem.MenuItemOptionId = option.Id;
                    newOrderItem.Name = string.Format("{0}/{1}", menuItem.Title, option.Title);
                    newOrderItem.Price = option.Price * quantity;
                }
                else
                {
                    newOrderItem.Name = menuItem.Title;
                    newOrderItem.Price = menuItem.Price * quantity;
                }

                total += newOrderItem.Price;
                this.Db.Insert(newOrderItem);
            }

            newOrder.TotalAmount = total;
            this.Db.Save();
            var notificationService = new NotificationService(new AirServiceEntityFrameworkContext(),
                                                              new ApnConnectionFactory());
            notificationService.NotifyVenueDevicesForNewOrder(venueId, newOrder.Id);
            if (this.Request.IsAjaxRequest())
            {
                return this.PartialView("_ConnectionList", this.GetCurrentConnections());
            }

            return this.GetDefaultView();
        }

        [HttpGet]
        public ActionResult Menus(int venueId)
        {
            if (!this.Request.IsAjaxRequest())
            {
                return this.GetDefaultView();
            }

            return this.PartialView("_Menus", this._menuService.GetMenus(venueId, null));
        }

        public ActionResult IPads(int venueId)
        {
            if (!this.Request.IsAjaxRequest())
            {
                return this.GetDefaultView();
            }

            return this.PartialView("_iPads", this.Db.Set<iPad>().Where(ipad => ipad.VenueId == venueId).ToList());
        }

        [HttpPost]
        public void SendMessageToCustomer(int venueId, int customerId, string message)
        {
            var notificationService = new NotificationService(new AirServiceEntityFrameworkContext(),
                                                              new ApnConnectionFactory());
            try{
                notificationService.SendMessageToCustomer(venueId,
                                                      customerId,
                                                      message);
            }
            catch
            {
                
            }
        }

        public ActionResult SimulateCloseRequest(int venueId,
                                                 int customerId)
        {
            try
            {
                this._orderService.RequestToFinalizeAllOrders(venueId,
                                                              customerId);
                return this.PartialView("_ConnectionList",
                                        this.GetCurrentConnections());
            }
            catch (Exception e)
            {
                Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                return new ContentResult
                           {
                               Content = e.Message
                           };
            }
        }

        public ActionResult ConfirmPickup(int customerId, int orderItemId)
        {
            try
            {
                this._orderService.ConfirmOrderItemPickup(customerId,
                                                          orderItemId); 
                return this.PartialView("_ConnectionList",
                                        this.GetCurrentConnections());
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return new ContentResult
                {
                    Content = e.Message
                };
            }
        }
    }
}