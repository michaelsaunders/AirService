<%@ Application Inherits="System.Web.HttpApplication" Language="C#" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="AirService.Data.Contracts" %> 
<%@ Import Namespace="AirService.Services.Notifications" %>
<%@ Import Namespace="AirService.Services.Security" %>
<%@ Import Namespace="Moq" %>
<%@ Import Namespace="Ninject" %>
<%@ Import Namespace="System.ServiceModel.Activation" %>
<%@ Import Namespace="Ninject.Extensions.Wcf" %>
<%@ Import Namespace="AirService.Model" %>
<%@ Import Namespace="System.Data.Entity" %>
<%@ Import Namespace="System.Collections.ObjectModel" %>
<%@ Import Namespace="System.Linq.Expressions" %>
<%@ Import Namespace="AirService.WebServices" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="AirService.WebServices.Framework" %>
<%@ Import Namespace="System.Data" %>

<script RunAt="server" Language="C#">
    protected void Application_Start(object sender, EventArgs e)
    {
        this.RegisterRoutes();
        KernelContainer.Kernel = new StandardKernel(new WebServiceNinjectModuleForTest());
        var logFile = this.Server.MapPath("~/Log/tracelog.svclog");
        if(File.Exists(logFile))
        {
            File.Delete(logFile);
        }
    }

    private void RegisterRoutes()
    {
        //var factory = new NinjectServiceHostFactory();
        var factory = new WebServiceHostFactory();
        RouteTable.Routes.Add(new ServiceRoute("Customer",
                                               factory,
                                               typeof (CustomerWebService)));
        RouteTable.Routes.Add(new ServiceRoute("Venue",
                                               factory,
                                               typeof (VenueWebService)));
    }

        #region Nested type: InMemoryDbSet

    /// <summary>
    /// https://gist.github.com/raw/913659/e54374a0994fbd556a9624eccd1eb092c25468b6/InMemoryDbSet.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InMemoryDbSet<T> : IDbSet<T> where T : class
    {
        private readonly IQueryable<T> _queryableSet;
        private readonly HashSet<T> _set;
        private int _autoIncrementId;
        private readonly List<T> _modified;
        private ObservableCollection<T> _localCollection;

        public InMemoryDbSet() : this(Enumerable.Empty<T>())
        {
        }

        public InMemoryDbSet(IEnumerable<T> entities)
        {
            this._set = new HashSet<T>();
            this._modified = new List<T>();
            foreach (T entity in entities)
            {
                this._set.Add(entity);
            }

            this._queryableSet = this._set.AsQueryable();
            this._localCollection = new ObservableCollection<T>();
        }

        /// <summary>
        /// Add item without tracking changes.
        /// </summary>
        /// <param name="entity"></param>
        internal T Insert(T entity)
        {
            var model = entity as SimpleModel;
            if (model != null && model.Id == 0)
            {
                model.Id = ++this._autoIncrementId;
            }

            this._set.Add(entity);
            return entity;
        }
        
        #region IDbSet<T> Members

        public T Add(T entity)
        {
            this.Insert(entity);
            this._modified.Add(entity);
            return entity;
        }

        public T Attach(T entity)
        {
            if (!this._set.Contains(entity))
            {
                this._set.Add(entity);
            }
            
            this._modified.Add(entity);
            return entity;
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            throw new NotImplementedException();
        }

        public T Create()
        {
            throw new NotImplementedException();
        }

        public T Find(params object[] keyValues)
        {
            return this._set.FirstOrDefault(item => keyValues[0].Equals((item as SimpleModel).Id));
        }

        public ObservableCollection<T> Local
        {
            get
            {
                return this._localCollection;
            }
        }

        public T Remove(T entity)
        {
            this._set.Remove(entity);
            return entity;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Type ElementType
        {
            get
            {
                return this._queryableSet.ElementType;
            }
        }

        public Expression Expression
        {
            get
            {
                return this._queryableSet.Expression;
            }
        }

        public IQueryProvider Provider
        {
            get
            {
                return this._queryableSet.Provider;
            }
        }

        #endregion

        internal List<T> Modified
        {
            get
            {
                return this._modified;
            }
        }
    }

        #endregion

        #region Nested type: WebServiceNinjectModuleForTest

    /// <summary>
    /// This class inherit from the real App Ninject module,  
    /// so that it all resolves service types to real service concrete class. 
    /// 
    /// What we do here is to just setup dummy model values 
    /// that are used by real repository and service classes
    /// </summary>
    private class WebServiceNinjectModuleForTest : WebServiceNinjectModule
    {
        public override void Load()
        {
            base.Load();
            var mockContext = new Mock<IAirServiceContext>();
            // customers
            this.AddTestCustomers(mockContext);
            // Venues 
            this.AddTestVenues(mockContext);

            // Venue connections
            var venueConnections = new InMemoryDbSet<VenueConnection>();
            mockContext.Setup(m => m.Set<VenueConnection>()).Returns(venueConnections);
            mockContext.SetupGet(m => m.VenueConnections).Returns(venueConnections);

            // setup some test menu)))
            this.AddTestMenus(mockContext);

            // Add test ipads to the one of test venue
            this.AddTestiPadForVenue(mockContext,
                                     1,
                                     1);
            this.AddTestiPadForVenue(mockContext,
                                     2,
                                     1);
            
            // add test areas to each venue
            this.AddTestVenueAreas(mockContext);
            // setup order
            var orders = new InMemoryDbSet<Order>();
            mockContext.Setup(m => m.Set<Order>()).Returns(orders);
            mockContext.SetupGet(m => m.Orders).Returns(orders);

            var orderItems = new InMemoryDbSet<OrderItem>();
            mockContext.Setup(m => m.Set<OrderItem>()).Returns(orderItems);
            mockContext.SetupGet(m => m.OrderItems).Returns(orderItems);

            
            var mockMembershipService = new Mock<IMembershipService>();
            var membershipUser1 = CreateAdminUser("venue1Admin", new Guid("4460CE56-0AFB-42CB-BEF5-502E58387934"));
            var membershipUser2 = CreateAdminUser("venue2Admin", new Guid("E5E9E63F-CAF8-4A9B-9CD2-760F8ECC7D34"));
            
            mockMembershipService.Setup(m => m.GetUser(It.Is<string>(name => name == "venue1Admin"),
                                                       It.Is<string>(pwd => pwd == "password"))).Returns(
                                                           membershipUser1);
            mockMembershipService.Setup(m => m.GetUser(It.Is<string>(name => name == "venue2Admin"),
                                                       It.Is<string>(pwd => pwd == "password"))).Returns(
                                                           membershipUser2);
            mockMembershipService.Setup(m => m.GetUserRoles(It.Is<string>(user => user == "venue1Admin" || user == "venue2Admin"))).Returns(new[]
                            {
                                WellKnownSecurityRoles.VenueAdministrators
                            });
            
            var serviceProviders = new InMemoryDbSet<ServiceProvider>(new []
                                                                          {
                                                                                new ServiceProvider
                                               {
                                                   UserId = (Guid)membershipUser1.ProviderUserKey,
                                                   Id = 1,
                                                   VenueId = 1, 
                                                   Venue = mockContext.Object.Venues.Find(1)
                                               }, 
                                           new ServiceProvider
                                               {
                                                   UserId = (Guid)membershipUser2.ProviderUserKey,
                                                   Id = 2,
                                                   VenueId = 2, 
                                                   Venue = mockContext.Object.Venues.Find(2)
                                               }});
            
            mockContext.Setup(m=>m.Set<ServiceProvider>()).Returns(serviceProviders);
            mockContext.SetupGet(m => m.ServiceProviders).Returns(serviceProviders);
            
            var tokens = new InMemoryDbSet<NotificationToken>();
            mockContext.Setup(m => m.Set<NotificationToken>()).Returns(tokens);
            mockContext.SetupGet(m => m.NotificationTokens).Returns(tokens);

            var notifications = new InMemoryDbSet<Notification>();
            mockContext.Setup(m => m.Set<Notification>()).Returns(notifications);
            mockContext.SetupGet(m => m.Notifications).Returns(notifications);

            var deviceMenuOptions = new InMemoryDbSet<DeviceMenuOption>();
            mockContext.Setup(m => m.Set<DeviceMenuOption>()).Returns(deviceMenuOptions);
            mockContext.SetupGet(m => m.DeviceMenuOptions).Returns(deviceMenuOptions);

            var deviceMenuItemOptions = new InMemoryDbSet<DeviceMenuItemOption>();
            mockContext.Setup(m => m.Set<DeviceMenuItemOption>()).Returns(deviceMenuItemOptions);
            mockContext.SetupGet(m => m.DeviceMenuItemOptions).Returns(deviceMenuItemOptions);
            
            this.AddVenueDeviceAdmin(mockContext, 1, "venue1DeviceAdmin@airservice.com");
            this.AddVenueDeviceAdmin(mockContext, 2, "venue2DeviceAdmin@airservice.com");

            mockContext.Setup(m => m.GetEntitiesByStates<AirService.Model.Menu>(It.IsAny<EntityState>())).Returns(() => ((InMemoryDbSet<AirService.Model.Menu>)mockContext.Object.Set<AirService.Model.Menu>()).Modified);
            mockContext.Setup(m => m.GetEntitiesByStates<MenuCategory>(It.IsAny<EntityState>())).Returns(() => ((InMemoryDbSet<MenuCategory>)mockContext.Object.Set<MenuCategory>()).Modified);
            mockContext.Setup(m => m.GetEntitiesByStates<AirService.Model.MenuItem>(It.IsAny<EntityState>())).Returns(() => ((InMemoryDbSet<AirService.Model.MenuItem>)mockContext.Object.Set<AirService.Model.MenuItem>()).Modified);
            mockContext.Setup(m => m.GetEntitiesByStates<MenuItemOption>(It.IsAny<EntityState>())).Returns(() => ((InMemoryDbSet<MenuItemOption>)mockContext.Object.Set<MenuItemOption>()).Modified);
            mockContext.Setup(m => m.SaveChanges()).Callback(() =>
                                                                 {
                                                                     ((InMemoryDbSet<AirService.Model.Menu>)mockContext.Object.Set<AirService.Model.Menu>()).Modified.Clear();
                                                                     ((InMemoryDbSet<MenuCategory>)mockContext.Object.Set<MenuCategory>()).Modified.Clear();
                                                                     ((InMemoryDbSet<AirService.Model.MenuItem>)mockContext.Object.Set<AirService.Model.MenuItem>()).Modified.Clear();
                                                                     ((InMemoryDbSet<MenuItemOption>)mockContext.Object.Set<MenuItemOption>()).Modified.Clear();
                                                                 });

            mockContext.Setup(m => m.Clone()).Returns(mockContext.Object);
            var mockApnConnection = new Mock<IApnConnection>();
            var mockApnConnectionFactory = new Mock<IApnConnectionFactory>();
            mockApnConnectionFactory.Setup(m => m.GetApnClientForCustomer(It.IsAny<IAirServiceContext>())).Returns(
                mockApnConnection.Object);
            mockApnConnectionFactory.Setup(m => m.GetApnClientForVenue(It.IsAny<IAirServiceContext>())).Returns(
                mockApnConnection.Object);
            this.Rebind<IAirServiceContext>().ToMethod(context => mockContext.Object);
            this.Rebind<IMembershipService>().ToMethod(context => mockMembershipService.Object);
            this.Rebind<IApnConnectionFactory>().ToMethod(context => mockApnConnectionFactory.Object);
        }

        private MembershipUser CreateAdminUser(string userName, Guid providerUserKey)
        {
            return new MembershipUser("AspNetSqlMembershipProvider",
                                      userName,
                                      providerUserKey,
                                      "test@test.com",
                                      "what",
                                      null,
                                      true,
                                      false,
                                      DateTime.Now,
                                      DateTime.Now,
                                      DateTime.Now,
                                      DateTime.Now,
                                      DateTime.MinValue);
        }

        private void AddTestCustomers(Mock<IAirServiceContext> mockContext)
        {
            var customers = new InMemoryDbSet<Customer>
                                {
                                    new Customer
                                        {
                                            CreateDate = DateTime.Now.AddYears(-1),
                                            UpdateDate = DateTime.Now.AddYears(-1),
                                            FacebookId = "1234",
                                            FirstName = "Inactive Customer",
                                            LastName = "Inactive Customer",
                                            Id = 999988,
                                            Image = "/inactive_customer.png",
                                            Status = (int) CmsStatus.Deleted,
                                            Udid = "6FA415B9-7A5F-4419-96D3-F5E6AB655B86"
                                        }
                                };

            mockContext.SetupGet(m => m.Customers).Returns(customers);
            mockContext.Setup(m => m.Set<Customer>()).Returns(customers);
        }

        private void AddTestVenues(Mock<IAirServiceContext> mockContext)
        {
            var venueSet = new InMemoryDbSet<Venue>(); 
            var venue = new Venue
                            {
                                Id = 1,
                                Title = "Test Venue 1",
                                Address1 = "Level 7 ",
                                Address2 = "1000 Pitt St",
                                Suburb = "Sydney",
                                State = new State
                                            {
                                                Title = "NSW"
                                            },
                                Country = new Country
                                              {
                                                  Title = "Australia"
                                              },
                                ContactFirstName = "Darren",
                                ContactLastName = "Clark",
                                Telephone = "98987676",
                                CreateDate = DateTime.Now,
                                UpdateDate = DateTime.Now,
                                LatitudePosition = -33.871627,
                                LongitudePosition = 151.208096,
                                VenueAccountType = (int)VenueAccountTypes.AccountTypeFull,
                                Status = (int) CmsStatus.Active, 
                                IsActive = true, 
                                MobileApplicationSettings = new MobileApplicationSettings
                                                                {
                                                                    HeaderImage = "/venues/testvenue1logo.jpg"
                                                                }
                            };
            venueSet.Insert(venue);

            venue = new Venue
                        {
                            Id = 2,
                            Title = "Test Venue 2",
                            Address1 = "Level 70",
                            Address2 = "1234 Wagalulu St",
                            Suburb = "Uruuru",
                            State = new State
                                        {
                                            Title = "Wellington"
                                        },
                            Country = new Country
                                          {
                                              Title = "Newzealand"
                                          },
                            ContactFirstName = "J",
                            ContactLastName = "J",
                            Telephone = "87878787",
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            LatitudePosition = -41.292494,
                            LongitudePosition = 174.773235,
                            VenueAccountType = (int)VenueAccountTypes.AccountTypeFull,
                            Status = (int) CmsStatus.Active,
                            IsActive = true,
                            MobileApplicationSettings = new MobileApplicationSettings
                                                                {
                                                                    HeaderImage = "/venues/testvenue2logo.jpg"
                                                                }
                        };
            venueSet.Insert(venue);

            venue = new Venue
                        {
                            Id = 3,
                            Title = "Inactive Venue 1",
                            Address1 = "Level 8 ",
                            Address2 = "1001 Pitt St",
                            Suburb = "Sydney",
                            State = new State
                                        {
                                            Title = "NSW"
                                        },
                            Country = new Country
                                          {
                                              Title = "Australia"
                                          },
                            ContactFirstName = "Darren",
                            ContactLastName = "Clark",
                            Telephone = "98987676",
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            LatitudePosition = -33.871627,
                            LongitudePosition = 151.208096,
                            VenueAccountType = (int)VenueAccountTypes.AccountTypeEvaluation,
                            Status = (int) CmsStatus.Deleted,
                            IsActive = true,
                            MobileApplicationSettings = new MobileApplicationSettings
                                                                {
                                                                    HeaderImage = "/venues/testvenue1logo.jpg"
                                                                }
                        };

            venueSet.Insert(venue);
            
             venue = new Venue
                        {
                            Id = 4,
                            Title = "Inactive Venue 1",
                            Address1 = "Level 8 ",
                            Address2 = "1001 Pitt St",
                            Suburb = "Sydney",
                            State = new State
                                        {
                                            Title = "NSW"
                                        },
                            Country = new Country
                                          {
                                              Title = "Australia"
                                          },
                            ContactFirstName = "Darren",
                            ContactLastName = "Clark",
                            Telephone = "98987676",
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            LatitudePosition = -33.826220,
                            LongitudePosition = 150.995750,
                            VenueAccountType = (int)VenueAccountTypes.AccountTypeEvaluation,
                            Status = (int) CmsStatus.Active,
                            IsActive = true,
                            MobileApplicationSettings = new MobileApplicationSettings
                                                                {
                                                                    HeaderImage = "/venues/testvenue1logo.jpg"
                                                                }
                        };

            venueSet.Insert(venue);
            
            venue = new Venue
                        {
                            Id = 9999,
                            Title = "Venue with airservice disabled",
                            Address1 = "Level 8 ",
                            Address2 = "1001 Pitt St",
                            Suburb = "Sydney",
                            State = new State
                                        {
                                            Title = "NSW"
                                        },
                            Country = new Country
                                          {
                                              Title = "Australia"
                                          },
                            ContactFirstName = "Darren",
                            ContactLastName = "Clark",
                            Telephone = "98987676",
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            LatitudePosition = -63.871627,
                            LongitudePosition = 251.208096,
                            VenueAccountType = (int)VenueAccountTypes.AccountTypeFull,
                            Status = (int) CmsStatus.Active,
                            IsActive = false, 
                            MobileApplicationSettings = new MobileApplicationSettings
                                                                {
                                                                    HeaderImage = "/venues/testvenue1logo.jpg"
                                                                }
                        };

            venueSet.Insert(venue); 
            var locationData = new[]
                                   {
                                       new object[]
                                           {
                                               -33.871912, 151.196036, "Pymont St"
                                           },
                                       new object[]
                                           {
                                               -33.871226, 151.204877, "Kent St"
                                           },
                                       new object[]
                                           {
                                               -33.870923, 151.206862, "Pitt St"
                                           },
                                       new object[]
                                           {
                                               -33.870941, 151.206079, "York St"
                                           },
                                       new object[]
                                           {
                                               -33.871618, 151.203933, "Sussex St"
                                           }
                                   };

            int id = 1000;
            foreach (var location in locationData)
            {
                venue = new Venue
                            {
                                Id = id++,
                                Title = (string) location[2],
                                Address1 = "Level 7 ",
                                Address2 = "1000 Pitt St",
                                Suburb = "Sydney",
                                State = new State
                                            {
                                                Title = "NSW"
                                            },
                                Country = new Country
                                              {
                                                  Title = "Australia"
                                              },
                                ContactFirstName = "Darren",
                                ContactLastName = "Clark",
                                Telephone = "98987676",
                                CreateDate = DateTime.Now,
                                UpdateDate = DateTime.Now,
                                LatitudePosition = (double) location[0],
                                LongitudePosition = (double) location[1],
                                VenueAccountType = (int)VenueAccountTypes.AccountTypeFull,
                                Status = (int) CmsStatus.Active, 
                                IsActive = true,
                                MobileApplicationSettings = new MobileApplicationSettings
                                                                {
                                                                    HeaderImage = "/venues/testvenue1logo.jpg"
                                                                }
                            };

                venueSet.Insert(venue);
            }

            mockContext.SetupGet(m => m.Venues).Returns(venueSet);
            mockContext.Setup(m => m.Set<Venue>()).Returns(venueSet);
        }

        private void AddTestMenus(Mock<IAirServiceContext> mockContext)
        {
            int nextMenuId = 1;
            int nextCategoryId = 1;
            int nextMenuItemId = 1;
            int nextMenuItemOptionId = 1;

            var menus = new InMemoryDbSet<AirService.Model.Menu>();
            var menuCategories = new InMemoryDbSet<MenuCategory>();
            var menuItems = new InMemoryDbSet<AirService.Model.MenuItem>();
            var menuItemOptions = new InMemoryDbSet<MenuItemOption>(); 
            
            mockContext.SetupGet(m=>m.Menus).Returns( menus);
            mockContext.Setup(m=>m.Set<AirService.Model.Menu>()).Returns(menus);
            
            mockContext.SetupGet(m=>m.MenuCategories).Returns( menuCategories);
            mockContext.Setup(m => m.Set<MenuCategory>()).Returns(menuCategories);
            
            mockContext.SetupGet(m=>m.MenuItems).Returns( menuItems);
            mockContext.Setup(m => m.Set<AirService.Model.MenuItem>()).Returns(menuItems);

            mockContext.SetupGet(m=>m.MenuItemOptions).Returns( menuItemOptions);
            mockContext.Setup(m => m.Set<MenuItemOption>()).Returns(menuItemOptions);

            int totalNon24HoursMenu = 0;
            foreach(var venue in mockContext.Object.Venues)
            { 
                for (int i = 0; i < 6; i++)
                {
                    var menu = new AirService.Model.Menu
                                   {
                                       Id = nextMenuId,
                                       Title = "Menu " + nextMenuId,
                                       Description = "Menu description " + nextMenuId, 
                                       ShowFrom = "00:00", 
                                       ShowTo = "23:59",
                                       Is24Hour = true,
                                       CreateDate = DateTime.Now,
                                       UpdateDate = DateTime.Now, 
                                       Venue = venue,
                                       VenueId = venue.Id,
                                       AssignedDevices = new List<DeviceMenuOption>(),
                                       MenuCategories = new List<MenuCategory>(), 
                                       MenuStatus = i != 2, 
                                       Status = i == 3
                                                ? SimpleModel.StatusFrozen
                                                : SimpleModel.StatusActive
                                   };
                    if( i >= 4)
                    {
                        if((totalNon24HoursMenu++ % 2)== 0 )
                        {
                            // for test purpose always make the time overlaps with the current time.
                            var time = DateTime.Now.AddHours(1);
                            menu.Is24Hour = false;
                            menu.ShowFrom = time.AddHours(-2).ToString("HHmm");
                            menu.ShowTo = time.ToString("HHmm");   
                        }
                        else
                        {
                            // for test purpose always make it avalable 6 hours ago.
                            var time = DateTime.Now.AddHours(-6);
                            menu.Is24Hour = false;
                            menu.ShowFrom = time.AddHours(-1).ToString("HHmm");
                            menu.ShowTo = time.ToString("HHmm");   
                        } 
                    }

                    menus.Insert(menu); 
                    for(int j = 0; j < 4; j++)
                    {
                        var menuCategory = new MenuCategory
                                               {
                                                   Id = nextCategoryId,
                                                   Title = "Menu category " + nextCategoryId,
                                                   CreateDate = DateTime.Now,
                                                   UpdateDate = DateTime.Now,
                                                   CategoryImage = "image.png",
                                                   Menu = menu,
                                                   MenuId = menu.Id, 
                                                   MenuItems = new List<AirService.Model.MenuItem>(),
                                                   IsLive = j != 2,
                                                   Status = i == 3
                                                                ? SimpleModel.StatusFrozen
                                                                : SimpleModel.StatusActive
                                               };
                        
                        menu.MenuCategories.Add(menuCategory);
                        menuCategories.Insert(menuCategory);
                        
                        for(int x = 0; x < 4; x++)
                        {
                            var menuItem = new AirService.Model.MenuItem
                                               {
                                                   Id = nextMenuItemId,
                                                   CreateDate = DateTime.Now,
                                                   UpdateDate = DateTime.Now,
                                                   Description = "Menu item description " + nextMenuItemId,
                                                   Title = "Menu item title " + nextMenuItemId,
                                                   Image = "menu item image.png",
                                                   MenuCategory = menuCategory,
                                                   MenuCategoryId = menuCategory.Id,
                                                   MenuItemOptions = new List<MenuItemOption>(), 
                                                   MenuItemStatus = x != 2 ,
                                                   Price = 99,
                                                   Status = x == 3
                                                                ? SimpleModel.StatusFrozen
                                                                : SimpleModel.StatusActive
                                               };
                            
                            menuCategory.MenuItems.Add(menuItem);
                            menuItems.Insert(menuItem);

                            for (int y = 0; y < 4; y++)
                            {
                                var option = new MenuItemOption
                                                 {
                                                     Id = nextMenuItemOptionId,
                                                     Title = "Option " + nextMenuItemOptionId,
                                                     CreateDate = DateTime.Now,
                                                     UpdateDate = DateTime.Now,
                                                     Price = 100,
                                                     Status = x == 3
                                                                  ? SimpleModel.StatusFrozen
                                                                  : SimpleModel.StatusActive
                                                 };

                                menuItem.MenuItemOptions.Add(option);
                                menuItemOptions.Insert(option);
                                nextMenuItemOptionId++;
                            }

                            nextMenuItemId++;
                        }

                        nextCategoryId++;
                    }

                    nextMenuId++;
                }
            }
        }
        
        /// <summary>
        /// Create a device user and associate first two ipads with the user.
        /// </summary>
        private void AddVenueDeviceAdmin(Mock<IAirServiceContext> mockContext,
                                         int venueId,
                                         string email)
        {
            var venue = mockContext.Object.Venues.Find(venueId);
            var dbSet = (InMemoryDbSet<DeviceAdmin>)mockContext.Object.DeviceAdmins;
            if(dbSet == null)
            {
                dbSet= new InMemoryDbSet<DeviceAdmin>();
                mockContext.Setup(m=>m.Set<DeviceAdmin>()).Returns(dbSet);
                mockContext.SetupGet(m=>m.DeviceAdmins).Returns(dbSet);
            }  
              
            var user = new DeviceAdmin
                           {
                               Venue = venue,
                               VenueId = venue.Id,
                               UserName = "Any name",
                               Email = email,
                               Password = "password",
                               iPads = new List<iPad>(venue.iPads.Skip(1).Take(2))
                           };
            
            dbSet.Insert(user);
        }
        
        private void AddTestiPadForVenue(Mock<IAirServiceContext> mockContext, int venueId, int firstiPadId)
        {
            var venue = mockContext.Object.Venues.Find(venueId);
            var menus =
                mockContext.Object.Menus.Where(
                    m => m.VenueId == venueId && m.Status == SimpleModel.StatusActive);
            var iPads = mockContext.Object.iPads as InMemoryDbSet<iPad>;
            if (iPads == null)
            { 
                iPads = new InMemoryDbSet<iPad>();
                mockContext.SetupGet(m => m.iPads).Returns(iPads);
                mockContext.Setup(m=>m.Set<iPad>()).Returns(iPads);
            }
            
            if(venue.iPads == null)
            {
                venue.iPads = new List<iPad>();
            }

            var firstiPad = new iPad
                           {
                               Name = "iPad 1",
                               Id = firstiPadId,
                               Pin = "1234",
                               VenueId = venueId,
                               Venue = venue, 
                               IsDeliveryEnabled = true, 
                               IsPickupEnabled = true
                           };

            firstiPad.AssignedMenus = (from menu in menus
                                       select new DeviceMenuOption
                                                  {
                                                      Menu = menu, 
                                                      MenuId = menu.Id, 
                                                      iPad = firstiPad, 
                                                      iPadId = firstiPadId
                                                  }).ToList();
            iPads.Insert(firstiPad);
            venue.iPads.Add(firstiPad);
            foreach (var realtion in firstiPad.AssignedMenus)
            {
                realtion.Menu.AssignedDevices.Add(realtion);
            }

            var secondiPad = new iPad
                           {
                               Name = "iPad 2",
                               Id = firstiPadId + 1,
                               Pin = "5678",
                               VenueId = venueId,
                               Venue = venue, 
                               IsDeliveryEnabled = true, 
                               IsPickupEnabled = true
                           };

            var secondiPadMenu = (from relation in firstiPad.AssignedMenus.Skip(1).Take(1)
                                  select new DeviceMenuOption
                                             {
                                                 Menu = relation.Menu,
                                                 MenuId = relation.MenuId,
                                                 iPad = secondiPad,
                                                 iPadId = secondiPad.Id
                                             }).ToList();

            secondiPad.AssignedMenus = secondiPadMenu;
            foreach (var option in secondiPadMenu)
            {
                option.Menu.AssignedDevices.Add(option);
            }
            
            iPads.Insert(secondiPad);
            venue.iPads.Add(secondiPad); 
            var thirdiPad = new iPad
                           {
                               Name = "iPad 3",
                               Id = firstiPadId + 2,
                               AssignedMenus = new List<DeviceMenuOption>(),
                               Pin = "0987",
                               VenueId = venueId,
                               Venue = venue, 
                               IsDeliveryEnabled = true, 
                               IsPickupEnabled = true
                           };

            iPads.Insert(thirdiPad);
            venue.iPads.Add(thirdiPad);

            var fourthiPad = new iPad
                           {
                               // use to test associating a new device to this data
                               Name = "iPad 4",
                               Id = firstiPadId + 3,
                               AssignedMenus = new List<DeviceMenuOption>(),
                               Pin = "0000",
                               VenueId = venueId,
                               Venue = venue, 
                               IsDeliveryEnabled = true, 
                               IsPickupEnabled = true
                           };
            iPads.Insert(fourthiPad);
            venue.iPads.Add(fourthiPad);
        }
        
        private void AddTestVenueAreas(Mock<IAirServiceContext> mockContext)
        {
            var venueAreas = new List<VenueArea>(); 
            foreach(var venue in mockContext.Object.Venues)
            {
                venue.VenueAreas = new List<VenueArea>();
                for(int i = 0; i < 3; i++)
                {
                    var venueArea = new VenueArea
                                        {
                                            CreateDate = DateTime.Now,
                                            UpdateDate = DateTime.Now,
                                            Description = string.Format("Area {0} of venue {1}",
                                                                        i + 1,
                                                                        venue.Id),
                                            Title = "Area " + (i + 1),
                                            Venue = venue, 
                                            VenueId = venue.Id,
                                            Status = SimpleModel.StatusActive
                                        };
                    venue.VenueAreas.Add(venueArea);
                    venueAreas.Add(venueArea);
                }
            }

            var dbSet = new InMemoryDbSet<VenueArea>(venueAreas);
            mockContext.Setup(m => m.Set<VenueArea>()).Returns(dbSet);
            mockContext.SetupGet(m => m.VenueAreas).Returns(dbSet);
        }
    }

#endregion

</script>