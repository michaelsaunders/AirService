using System.ServiceModel;
using AirService.Data;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services;
using AirService.Services.Contracts;
using AirService.Services.Eway;
using AirService.Services.Notifications;
using AirService.Services.Security;
using Ninject.Extensions.Wcf;
using Ninject.Modules;
using Ninject.Syntax;

namespace AirService.WebServices.Framework
{
    public class WebServiceNinjectModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IResolutionRoot>().ToConstant(this.Kernel);
            this.Bind<ServiceHost>().To<NinjectServiceHost>();
            this.Bind<IAirServiceContext>().To<AirServiceEntityFrameworkContext>().InRequestScope();

            // Customer
            this.Bind<IRepository<Customer>>().To<CustomerRepository>();
            this.Bind<IRepository<VenueConnection>>().To<VenueConnectionRepository>();
            this.Bind<ICustomerService>().To<CustomerService>();

            // Venue
            this.Bind<IRepository<Venue>>().To<VenueRepository>();
            this.Bind<IVenueService>().To<VenueService>();
            this.Bind<IRepository<VenueArea>>().To<VenueAreaRepository>(); 
            this.Bind<IVenueAreaService>().To<VenueAreaService>();
            this.Bind<IRepository<MobileApplicationSettings>>().To<MobileApplicationSettingsRepository>(); 
            // Venue's ipad service.  )
            this.Bind<IRepository<iPad>>().To<IPadRepository>();
            this.Bind<IIPadService>().To<iPadService>();

            // Membership auth service
            this.Bind<IMembershipService>().To<AccountMembershipService>();  
            
            this.Bind<IRepository<Menu>>().To<MenuRepository>();
            this.Bind<IMenuService>().To<MenuService>();
            this.Bind<IRepository<MenuCategory>>().To<MenuCategoryRepository>();
            this.Bind<IRepository<MenuItem>>().To<MenuItemRepository>();
            this.Bind<IRepository<MenuItemOption>>().To<MenuItemOptionRepository>();
            this.Bind<IMenuCategoryService>().To<MenuCategoryService>();
            this.Bind<IMenuItemService>().To<MenuItemService>();
            this.Bind<IMenuItemOptionService>().To<MenuItemOptionService>();
            this.Bind<IRepository<Order>>().To<OrderRepository>();
            this.Bind<IOrderService>().To<OrderService>();
            this.Bind<IRepository<VenueType>>().To<VenueTypeRepository>();
            this.Bind<IRepository<ServiceProvider>>().To<ServiceProviderRepository>();
            this.Bind<IServiceProviderService>().To<ServiceProviderService>();
            this.Bind<IRepository<DeviceAdmin>>().To<DeviceAdminRepository>();
            this.Bind<IDeviceAdminService>().To<DeviceAdminService>();
            this.Bind<IRepository<Country>>().To<CountryRepository>();
            this.Bind<IService<Country>>().To<CountryService>();
            this.Bind<IRepository<State>>().To<StateRepository>();
            this.Bind<IStateService>().To<StateService>();
            this.Bind<IMobileApplicationSettingsService>().To<MobileApplicationSettingsService>();
            this.Bind<IService<VenueType>>().To<VenueTypeService>();
            this.Bind<IRepository<Notification>>().To<NotificationRepository>();
            this.Bind<INotificationService>().To<NotificationService>(); 
            this.Bind<IApnConnectionFactory>().To<ApnConnectionFactory>();
            this.Bind<IEwayWrapper>().To<EwayWrapper>();
            this.Bind<IRepository<FailedPayment>>().To<SimpleRepository<FailedPayment>>();
            this.Bind<IPaymentService>().To<PaymentService>();
        }
    }
}