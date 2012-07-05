using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using AirService.Services.Security;
using AirService.Web.Infrastructure.Filters;
using log4net;

namespace AirService.Web
{

    public class MvcApplication : HttpApplication
    {
        public override void Init()
        {
            this.PostAuthenticateRequest += this.MvcApplication_PostAuthenticateRequest;
            base.Init();
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MustBeTrueAttribute());
            //filters.Add(new LogonAuthorizeAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            ILog logger = LogManager.GetLogger(typeof(MvcApplication));
            logger.Info("Application started.");

            AreaRegistration.RegisterAllAreas(); 
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes); 
        }

        protected void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                string encryptedTicket = authCookie.Value;
                if (!String.IsNullOrWhiteSpace(encryptedTicket))
                {
                    var authTicket = FormsAuthentication.Decrypt(encryptedTicket);
                    var airServiceIdentity = new AirServiceIdentity(authTicket);
                    var user = Membership.GetUser(airServiceIdentity.UserId);
                    if (user != null && user.IsApproved)
                    {
                        var principal = new RolePrincipal(Roles.Provider.Name, airServiceIdentity);
                        this.Context.User = principal;
                    }
                    else
                    {
                        this.Context.User = new GenericPrincipal(new GenericIdentity(""), new string[0]);
                        Context.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                    }
                }
            } 
        }
    }
}