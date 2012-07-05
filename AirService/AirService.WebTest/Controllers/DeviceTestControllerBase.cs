using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services;
using AirService.Services.Notifications;
using AirService.Services.Security;
using AirService.WebTest.ViewModels;

namespace AirService.WebTest.Controllers
{
    public class DeviceTestControllerBase<T> : Controller where T : class
    {
        private readonly IRepository<T> _db;

        protected DeviceTestControllerBase(IRepository<T> db)
        {
            this._db = db;
        }

        protected IRepository<T> Db
        {
            get
            {
                return this._db;
            }
        }

        [HttpPost]
        public ActionResult CreateNewVenue(string venueName,
                                           int accountType,
                                           string venueLatitude,
                                           string venueLongitude)
        {
            this.CreateNewTestVenue(venueName, accountType, venueLatitude, venueLongitude);
            if (this.Request.IsAjaxRequest())
            {
                return this.PartialView("_VenueList", this.GetServiceProviders());
            }

            return this.GetDefaultView();
        }

        [HttpPost]
        public ActionResult Connections(int? venueId,
                                        int? customerId)
        {
            if (this.Request.IsAjaxRequest())
            {
                if (venueId.HasValue && customerId.HasValue)
                {
                    VenueConnection connection = (from c in this.Db.Set<VenueConnection>()
                                                  where c.CustomerId == customerId.Value &&
                                                        c.VenueId == venueId.Value && 
                                                        c.Status != VenueConnection.Disconnected
                                                  select c).FirstOrDefault();
                    VenueConnection newConnection = null;
                    if (connection == null)
                    {
                        Venue venue = this.Db.Set<Venue>().Find(venueId.Value);
                        Customer customer = this.Db.Set<Customer>().Find(customerId.Value);
                        newConnection = new VenueConnection
                                                {
                                                    CreateDate = DateTime.Now,
                                                    UpdateDate = DateTime.Now,
                                                    Customer = customer,
                                                    CustomerId = customerId.Value,
                                                    Venue = venue,
                                                    VenueId = venueId.Value
                                                };

                        this.Db.Set<VenueConnection>().Add(newConnection); 
                    }
                    else
                    {
                        this.Db.Update(connection);
                    }

                    this.Db.Save();
                    if (newConnection != null)
                    {
                        new NotificationService(this.Db.Context, new ApnConnectionFactory()).NotifyVenueDevicesForNewConnectionRequest(newConnection.Id);
                    }
                }

                return this.PartialView("_ConnectionList",
                                        this.GetCurrentConnections());
            }

            return this.GetDefaultView();
        }

        public ActionResult VenueCustomerOrders(int venueId,
                                                int customerId)
        {
            if (!this.Request.IsAjaxRequest())
            {
                return this.GetDefaultView();
            }

            this.Db.SetContextOption(ContextOptions.LazyLoading, false);
            var currentConnection = (from c in this.Db.Set<VenueConnection>()
                                     where c.CustomerId == customerId &&
                                           c.VenueId == venueId &&
                                           c.Status != VenueConnection.Disconnected
                                     select c).FirstOrDefault();
            var orders = this.Db.Set<Order>()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Include("OrderItems.iPad");
            var model = new CustomerOrderViewModel(venueId,
                                                   from order in orders
                                                   where order.VenueId == venueId &&
                                                         order.CustomerId == customerId &&
                                                         order.CreateDate > currentConnection.CreateDate
                                                   orderby order.UpdateDate descending
                                                   select order);

            if (model.Customer == null || !model.Orders.Any())
            {
                return new ContentResult {Content = "No Orders"};
            }

            return this.PartialView("_CustomerOrders",
                                    model);
        }

        public ActionResult Customers()
        {
            if (Request.IsAjaxRequest())
            {
                return this.PartialView("_CustomerList", this.GetCurrentCustomers());
            }

            return this.GetDefaultView();
        }

        protected List<CustomerViewModel> GetCurrentCustomers()
        {
            IQueryable<CustomerViewModel> query = from customer in this._db.Set<Customer>()
                                                  join connection in this._db.Set<VenueConnection>() on customer.Id
                                                      equals connection.CustomerId into g
                                                  orderby customer.UpdateDate descending
                                                  select new CustomerViewModel
                                                             {
                                                                 Customer = customer,
                                                                 Connections =
                                                                     (from connection in g.DefaultIfEmpty()
                                                                      select connection)
                                                             };
            return query.ToList();
        }

        protected List<ServiceProvider> GetServiceProviders()
        {
            IOrderedQueryable<ServiceProvider> query =
                from provider in
                    this._db.Set<ServiceProvider>().Include(sp => sp.Venue)
                orderby provider.CreateDate descending
                select provider;
            return query.ToList();
        }

        private List<DeviceAdmin> GetDeviceAdmins()
        {
            return (from admin in this._db.Set<DeviceAdmin>()
                    orderby admin.Venue.Title
                    select admin).ToList();
        }

        protected List<VenueConnectionViewModel> GetCurrentConnections()
        {
            return (from connection in this._db.Set<VenueConnection>()
                    join order in this._db.Set<Order>() on
                        new
                            {
                                customerId = connection.CustomerId,
                                venueId = connection.VenueId
                            }
                        equals
                        new
                            {
                                customerId = order.CustomerId,
                                venueId = order.VenueId
                            } into g
                    where connection.Status != VenueConnection.Disconnected
                    orderby connection.UpdateDate descending
                    select new VenueConnectionViewModel
                               {
                                   Venue = connection.Venue,
                                   Customer = connection.Customer,
                                   Connection = connection,
                                   TotalOrders =
                                       g.DefaultIfEmpty().Count(o => o != null && o.CreateDate > connection.CreateDate)
                               }).ToList();
        }

        protected List<Menu> CreateRandomMenu(int venueId)
        {
            List<iPad> iPads = this._db.Set<iPad>().Where(ipad => ipad.VenueId == venueId).ToList();
            if (iPads.Count == 0)
            {
                iPads = this.CreateRandomiPads(venueId);
            }

            var menus = new List<Menu>();
            for (int i = 0; i < 3; i++)
            {
                var menu = new Menu
                               {
                                   Title = Utility.GetRandomString(3, 10),
                                   Description = Utility.GetRandomString(10, 20),
                                   DisplayTitle = Utility.GetRandomString(10, 20),
                                   VenueId = venueId,
                                   MenuStatus = true,
                                   Status = SimpleModel.StatusActive,
                                   CreateDate = DateTime.Now,
                                   UpdateDate = DateTime.Now,
                                   MenuCategories = new List<MenuCategory>(),
                                   AssignedDevices = new List<DeviceMenuOption>()
                               };

                iPad selectediPad = iPads.FirstOrDefault(ipad => ipad.AssignedMenus.Count == 0) ??
                                    iPads[Utility.Random.Next(0, 2)];
                var relation = new DeviceMenuOption
                                   {
                                       Menu = menu,
                                       MenuId = menu.Id,
                                       iPad = selectediPad,
                                       iPadId = selectediPad.Id, 
                                       Print = true
                                   };
                selectediPad.AssignedMenus.Add(relation);
                menu.AssignedDevices.Add(relation);
                this._db.Update(selectediPad);

                menus.Add(menu);
                this._db.Insert(menu);
                for (int j = 0; j < 3; j++)
                {
                    var category = new MenuCategory
                                       {
                                           Title = Utility.GetRandomString(3, 10),
                                           CategoryImage =
                                               this.Url.Content("~/content/image0") + Utility.Random.Next(1, 6) + ".png",
                                           IsLive = true,
                                           Menu = menu,
                                           Status = SimpleModel.StatusActive,
                                           CreateDate = DateTime.Now,
                                           UpdateDate = DateTime.Now,
                                           MenuItems = new List<MenuItem>()
                                       };
                    menu.MenuCategories.Add(category);
                    this._db.Insert(category);

                    for (int k = 0; k < 3; k++)
                    {
                        Decimal price = Utility.Random.Next(10, 100);
                        var item = new MenuItem
                                       {
                                           Title = Utility.GetRandomString(3, 10),
                                           Description = Utility.GetRandomString(10, 20),
                                           Image =
                                               this.Url.Content("~/content/image0") + Utility.Random.Next(1, 6) + ".png",
                                           Price = price,
                                           MenuItemStatus = true,
                                           MenuCategory = category,
                                           Status = SimpleModel.StatusActive,
                                           CreateDate = DateTime.Now,
                                           UpdateDate = DateTime.Now,
                                           MenuItemOptions = new List<MenuItemOption>()
                                       };

                        category.MenuItems.Add(item);
                        this._db.Insert(item);
                        for (int x = 0; x < 2; x++)
                        {
                            decimal optionPrice = price * (decimal) (1 + Utility.Random.Next(10, 30) / 100.0);
                            var option = new MenuItemOption
                                             {
                                                 Title = Utility.GetRandomString(3, 10),
                                                 Price = optionPrice,
                                                 MenuItem = item,
                                                 Status = SimpleModel.StatusActive,
                                                 CreateDate = DateTime.Now,
                                                 UpdateDate = DateTime.Now
                                             };

                            item.MenuItemOptions.Add(option);
                            this._db.Insert(item);
                        }
                    }
                }
            }

            this._db.Save();
            return menus;
        }

        protected virtual void CreateNewTestVenue(string venueName,
                                                  int accountType,
                                                  string venueLatitude,
                                                  string venueLongitude)
        {
            double lat, lng;
            if (!double.TryParse(venueLatitude, out lat))
            {
                lat = -33.872194;
            }

            if (!double.TryParse(venueLongitude, out lng))
            {
                lng = 151.208166;
            }

            var setting = new MobileApplicationSettings
                              {
                                  UpdateDate = DateTime.Now, 
                                  CreateDate = DateTime.Now, 
                                  Status = SimpleModel.StatusActive
                              };
            var title = string.IsNullOrWhiteSpace(venueName) ? Utility.GetRandomString(10) : venueName;
            var venue = new Venue
                            {
                                CreateDate = DateTime.Now,
                                UpdateDate = DateTime.Now,
                                Title = title,
                                Description = title,
                                ContactFirstName = Utility.GetRandomString(5),
                                ContactLastName = Utility.GetRandomString(5),
                                Telephone = Utility.GetRandomString(10),
                                LatitudePosition = lat,
                                LongitudePosition = lng,
                                Address1 = "Test Address1",
                                Suburb = "Test Suburb", 
                                StateId = 1,
                                Postcode = "1234",
                                Status = SimpleModel.StatusActive,
                                IsActive = true,
                                CountryId = 1,
                                VenueAccountType = accountType,
                                MobileApplicationSettings = setting
                            };

            this.Db.Set<Venue>().Add(venue);
            this.Db.Set<MobileApplicationSettings>().Add(setting);
            this.Db.Save();

            this.CreateRandomMenu(venue.Id);
            MembershipUser user = Membership.CreateUser(Utility.GetRandomString(10), "password",
                                                        Utility.GetRandomEmail());

            var providerUserKey = (Guid) user.ProviderUserKey;
            string userName = providerUserKey.ToString("N").ToLower();
            this.Db.ExecuteCommand(
                @"
update dbo.aspnet_Users
set userName = {0}, 
loweredUserName = {1}
where userid = {2}",
                userName,
                userName,
                providerUserKey
                );

            user = Membership.GetUser(userName);   
            Roles.AddUserToRole(user.UserName,
                                WellKnownSecurityRoles.VenueAdministrators);
            var serviceProvider = new ServiceProvider
                                      {
                                          VenueId = venue.Id,
                                          UserId = providerUserKey,
                                          CreateDate = DateTime.Now,
                                          UpdateDate = DateTime.Now,
                                          Status = SimpleModel.StatusActive
                                      };

            this.Db.Set<ServiceProvider>().Add(serviceProvider);
            this.Db.Save();
        }

        protected List<iPad> CreateRandomiPads(int venueId)
        {
            var ipads = new List<iPad>();
            for (int i = 0; i < 3; i++)
            {
                var ipad = new iPad
                               {
                                   Name = Utility.GetRandomString(3, 10),
                                   Image = this.Url.Content("~/content/image0") + Utility.Random.Next(1, 6) + ".png",
                                   Pin = Utility.Random.Next(100000, 999999).ToString(),
                                   Status = SimpleModel.StatusActive,
                                   Location = Utility.GetRandomString(5, 10),
                                   IsDeliveryEnabled = true,
                                   IsPickupEnabled = Utility.Random.Next(0, 2) == 1,
                                   VenueId = venueId,
                                   CreateDate = DateTime.Now,
                                   UpdateDate = DateTime.Now,
                                   AssignedMenus = new List<DeviceMenuOption>()
                               };

                ipads.Add(ipad);
                this._db.Insert(ipad);
            }

            this._db.Save();
            return ipads;
        }

        protected ActionResult GetDefaultView()
        {
            var model = new DeviceTestViewModel
                            {
                                Connections =
                                    this.GetCurrentConnections(),
                                Customers = this.GetCurrentCustomers(),
                                ServiceProviders = this.GetServiceProviders(), 
                                DeviceAdmins = this.GetDeviceAdmins()
                            };

            return this.View("Index",
                             model);
        } 
    }
}