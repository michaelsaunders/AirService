using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using AirService.Data.Contracts;
using AirService.Model;
using AirService.Services.Security;
using Ninject;

namespace AirService.WebTest
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // wildcard mapping for action names?
            routes.MapRoute(
                "ProcessOrderItem", // Route name
                "iPhoneTest/ProcessOrderItem/{orderItemId}", // URL with parameters
                new { controller = "iPhoneTest", action = "ProcessOrderItem", orderItemId = @"\d+"}
                );

            routes.MapRoute(
                "ConfirmOrderItem", // Route name
                "iPhoneTest/ConfirmOrderItem/{orderItemId}", // URL with parameters
                new { controller = "iPhoneTest", action = "ConfirmOrderItem", orderItemId = @"\d+" }
                );

            routes.MapRoute(
                "FinaizeCustomerOrder", // Route name
                "iPhoneTest/FinaizeCustomerOrder/{customerId}", // URL with parameters
                new { controller = "iPhoneTest", action = "FinaizeCustomerOrder", customerId = @"\d+" }
                );

            routes.MapRoute(
                "VenueId", // Route name
                "{controller}/{action}/{venueId}", // URL with parameters
                new {controller = "Home", action = "Index", venueId = @"\d+"} 
                );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            try
            {
                this.SetupMembershipAndRoles();
            }
            catch
            { 
            }
        }

        [Conditional("DEBUG")]
        private void SetupMembershipAndRoles()
        {
            string[] roles = Roles.GetAllRoles();
            if (!roles.Contains(WellKnownSecurityRoles.VenueAdministrators))
            {
                Roles.CreateRole(WellKnownSecurityRoles.VenueAdministrators);
            }

            if (!roles.Contains(WellKnownSecurityRoles.VenueUsers))
            {
                Roles.CreateRole(WellKnownSecurityRoles.VenueUsers);
            }

            if (!roles.Contains(WellKnownSecurityRoles.Customers))
            {
                Roles.CreateRole(WellKnownSecurityRoles.Customers);
            }

            if (!roles.Contains(WellKnownSecurityRoles.SystemAdministrators))
            {
                Roles.CreateRole(WellKnownSecurityRoles.SystemAdministrators);
            }


            // AirServiceInitializer no longer add default country & state 
            using (var context = new AirServiceEntityFrameworkContext())
            {
                var country = context.Countries.Find(1);
                if (country == null)
                {
                    country = new Country
                                  {
                                      CreateDate = DateTime.Now,
                                      UpdateDate = DateTime.Now,
                                      Title = "Australia",
                                      Status = SimpleModel.StatusActive
                                  };

                    context.Countries.Add(country);
                    context.SaveChanges();
                }

                if (context.States.Count() == 0)
                {
                    context.States.Add(new State
                                           {
                                               Country = country,
                                               CreateDate = DateTime.Now,
                                               UpdateDate = DateTime.Now,
                                               Status = SimpleModel.StatusActive,
                                               Title = "NSW"
                                           });

                    context.States.Add(new State
                                           {
                                               Country = country,
                                               CreateDate = DateTime.Now,
                                               UpdateDate = DateTime.Now,
                                               Status = SimpleModel.StatusActive,
                                               Title = "TAS"
                                           });
                    context.SaveChanges();
                }
            }
        }
    }
}