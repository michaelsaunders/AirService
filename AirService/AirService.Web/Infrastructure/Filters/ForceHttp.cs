using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace AirService.Web.Infrastructure.Filters
{
    public class ForceHttpAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.IsSecureConnection)
            {
                var requireHttps =
                    filterContext.ActionDescriptor.GetCustomAttributes(typeof (RequireHttpsAttribute), true).
                        FirstOrDefault();
                if (requireHttps == null)
                {
                    UriBuilder uriBuilder = new UriBuilder(filterContext.RequestContext.HttpContext.Request.Url);
                    uriBuilder.Scheme = "http";
                    int port;
                    if (!int.TryParse(ConfigurationManager.AppSettings["HTTPPort"], out port))
                    {
                        port = 80;
                    }

                    uriBuilder.Port = port;
                    filterContext.Result = new RedirectResult(uriBuilder.Uri.AbsoluteUri);
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
