using AirService.Data;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;
using AirService.Services.Eway;
using AirService.Services.Notifications;
using AirService.Services.Security;
using AirService.WebTest.Framework;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Mvc;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(NinjectMVC3), "Stop")]

namespace AirService.WebTest.Framework
{ 
    public static class NinjectMVC3 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestModule));
            DynamicModuleUtility.RegisterModule(typeof(HttpApplicationInitializationModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IAirServiceContext>().To<AirServiceEntityFrameworkContext>().InRequestScope();
            // Customer
            kernel.Bind<IRepository<Customer>>().To<CustomerRepository>();
            kernel.Bind<IRepository<VenueConnection>>().To<VenueConnectionRepository>();
            kernel.Bind<ICustomerService>().To<CustomerService>();

            // Venue
            kernel.Bind<IRepository<Venue>>().To<VenueRepository>();
            kernel.Bind<IVenueService>().To<VenueService>();
            kernel.Bind<IRepository<VenueArea>>().To<VenueAreaRepository>(); 
            kernel.Bind<IVenueAreaService>().To<VenueAreaService>();

            // Venue's ipad service.  )
            kernel.Bind<IRepository<iPad>>().To<IPadRepository>();
            kernel.Bind<IIPadService>().To<iPadService>();

            // Membership auth service
            kernel.Bind<IMembershipService>().To<AccountMembershipService>(); 
            kernel.Bind<IRepository<Menu>>().To<MenuRepository>();
            kernel.Bind<IMenuService>().To<MenuService>();
            kernel.Bind<IRepository<MenuCategory>>().To<MenuCategoryRepository>();
            kernel.Bind<IRepository<MenuItem>>().To<MenuItemRepository>();
            kernel.Bind<IRepository<MenuItemOption>>().To<MenuItemOptionRepository>();
            kernel.Bind<IMenuCategoryService>().To<MenuCategoryService>();
            kernel.Bind<IMenuItemService>().To<MenuItemService>();
            kernel.Bind<IMenuItemOptionService>().To<MenuItemOptionService>();
            kernel.Bind<IRepository<Order>>().To<OrderRepository>();
            kernel.Bind<IOrderService>().To<OrderService>();
            kernel.Bind<IRepository<VenueType>>().To<VenueTypeRepository>();
            kernel.Bind<IRepository<ServiceProvider>>().To<ServiceProviderRepository>();
            kernel.Bind<IServiceProviderService>().To<ServiceProviderService>();
            kernel.Bind<IRepository<DeviceAdmin>>().To<DeviceAdminRepository>();
            kernel.Bind<IDeviceAdminService>().To<DeviceAdminService>();
            kernel.Bind<IRepository<Country>>().To<CountryRepository>();
            kernel.Bind<IService<Country>>().To<CountryService>();
            kernel.Bind<IRepository<State>>().To<StateRepository>();
            kernel.Bind<IStateService>().To<StateService>();
            kernel.Bind<IMobileApplicationSettingsService>().To<MobileApplicationSettingsService>();
            kernel.Bind<IService<VenueType>>().To<VenueTypeService>();
            kernel.Bind<IRepository<Notification>>().To<NotificationRepository>();
            kernel.Bind<INotificationService>().To<NotificationService>();
            kernel.Bind<IApnConnectionFactory>().To<ApnConnectionFactory>();
            kernel.Bind<IEwayWrapper>().To<EwayWrapper>();
            kernel.Bind<IRepository<FailedPayment>>().To<SimpleRepository<FailedPayment>>();
            kernel.Bind<IPaymentService>().To<PaymentService>();
        }        
    }
}
