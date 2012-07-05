using System.Web.Mvc;

namespace AirService.Web.Infrastructure.Filters
{
    public sealed class LogonAuthorizeAttribute : AuthorizeAttribute {
        public override void OnAuthorization(AuthorizationContext filterContext) {
            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
            || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);
            if (!skipAuthorization) {
                base.OnAuthorization(filterContext);
            }
        }
    }
}