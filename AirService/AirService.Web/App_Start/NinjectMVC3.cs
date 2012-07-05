using AirService.Data;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;
using AirService.Services.Eway;
using AirService.Services.Notifications;
using AirService.Services.Security;
using AutoMapper;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Mvc;

[assembly: WebActivator.PreApplicationStartMethod(typeof(AirService.Web.App_Start.NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(AirService.Web.App_Start.NinjectMVC3), "Stop")]

namespace AirService.Web.App_Start
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
            InitializeMapper();
        }

        private static void InitializeMapper()
        {
            Mapper.CreateMap<rebillTransaction, VenueTransaction>()
                .ForMember(r => r.Note, options => options.ResolveUsing(r => r.TransactionError));
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
            kernel.Bind<IRepository<Venue>>().To<VenueRepository>();
            kernel.Bind<IVenueService>().To<VenueService>();
            kernel.Bind<IRepository<VenueArea>>().To<VenueAreaRepository>();
            kernel.Bind<IVenueAreaService>().To<VenueAreaService>();
            kernel.Bind<IRepository<Country>>().To<CountryRepository>();
            kernel.Bind<IService<Country>>().To<CountryService>();
            kernel.Bind<IRepository<State>>().To<StateRepository>();
            kernel.Bind<IStateService>().To<StateService>(); 
            kernel.Bind<IRepository<Customer>>().To<CustomerRepository>();
            kernel.Bind<IService<Customer>>().To<CustomerService>();
            kernel.Bind<IRepository<Menu>>().To<MenuRepository>();
            kernel.Bind<IMenuService>().To<MenuService>();
            kernel.Bind<IMenuItemService>().To<MenuItemService>();
            kernel.Bind<IMenuItemOptionService>().To<MenuItemOptionService>();
            kernel.Bind<IMenuCategoryService>().To<MenuCategoryService>();
            kernel.Bind<IRepository<MenuCategory>>().To<MenuCategoryRepository>();
            kernel.Bind<IRepository<MenuItem>>().To<MenuItemRepository>();
            kernel.Bind<IRepository<MenuItemOption>>().To<MenuItemOptionRepository>();
            kernel.Bind<IRepository<Order>>().To<OrderRepository>();
            kernel.Bind<IService<Order>>().To<OrderService>();
            kernel.Bind<IRepository<OrderItem>>().To<OrderItemRepository>();
            kernel.Bind<IService<OrderItem>>().To<OrderItemService>();
            kernel.Bind<IRepository<ServiceProvider>>().To<ServiceProviderRepository>();
            kernel.Bind<IServiceProviderService>().To<ServiceProviderService>();
            kernel.Bind<IRepository<iPad>>().To<IPadRepository>();
            kernel.Bind<IIPadService>().To<iPadService>();
            kernel.Bind<IRepository<VenueConnection>>().To<VenueConnectionRepository>();
            kernel.Bind<IService<MobileApplicationSettings>>().To<MobileApplicationSettingsService>();
            kernel.Bind<IRepository<MobileApplicationSettings>>().To<MobileApplicationSettingsRepository>();
            kernel.Bind<IRepository<DeviceAdmin>>().To<DeviceAdminRepository>();
            kernel.Bind<IDeviceAdminService>().To<DeviceAdminService>();
            kernel.Bind<IRepository<VenueAdvertisement>>().To<VenueAdvertisementRepository>();
            kernel.Bind<IService<VenueType>>().To<VenueTypeService>();
            kernel.Bind<IRepository<VenueType>>().To<VenueTypeRepository>();
            kernel.Bind<VenueAdvertisementService>().To<VenueAdvertisementService>(); 
            kernel.Bind<IRepository<Notification>>().To<NotificationRepository>();
            kernel.Bind<INotificationService>().To<NotificationService>();
            kernel.Bind<IVenueReportService>().To<VenueReportService>();
            kernel.Bind<IMembershipService>().To<AccountMembershipService>();
            kernel.Bind<IApnConnectionFactory>().To<ApnConnectionFactory>();
            kernel.Bind<IEwayWrapper>().To<EwayWrapper>();
            kernel.Bind<IRepository<FailedPayment>>().To<SimpleRepository<FailedPayment>>();
            kernel.Bind<IPaymentService>().To<PaymentService>();
        }
    }
}
