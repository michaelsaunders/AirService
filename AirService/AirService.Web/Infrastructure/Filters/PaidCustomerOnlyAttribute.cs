using System.Web;
using System.Web.Mvc;
using AirService.Model;
using AirService.Services.Contracts;
using AirService.Services.Security;

namespace AirService.Web.Infrastructure.Filters
{
    public class PaidCustomerOnlyAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var identity = filterContext.HttpContext.User.Identity as AirServiceIdentity;
            if (identity != null)
            {
                if(identity.AccountType != VenueAccountTypes.AccountTypeFull)
                {
                    filterContext.Result = new RedirectResult("~/Account/Activate");
                    return;
                }
            }

            base.HandleUnauthorizedRequest(filterContext);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (base.AuthorizeCore(httpContext))
            {
                var user = httpContext.User.Identity as AirServiceIdentity;
                if (user != null)
                {
                    var service = DependencyResolver.Current.GetService<IVenueService>();
                    var venue = service.Find(user.VenueId);
                    user.AccountType = (VenueAccountTypes) venue.VenueAccountType;
                    if (venue.IsPaidAccount)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
