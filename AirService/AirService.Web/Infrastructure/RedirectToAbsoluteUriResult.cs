using System;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AirService.Web.Infrastructure
{
    public class RedirectToAbsoluteUriResult : ActionResult
    {
        private readonly string _actionName;
        private readonly string _controllerName;
        private readonly RouteValueDictionary _routeValues;
        private readonly bool _useSecureChannel;

        public RedirectToAbsoluteUriResult(string actionName,
                                           string controllerName,
                                           RouteValueDictionary routeValues,
                                           bool useSecureChannel)
        {
            this._actionName = actionName;
            this._controllerName = controllerName;
            this._routeValues = routeValues;
            this._useSecureChannel = useSecureChannel;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var requestUrl = context.HttpContext.Request.Url;
            Contract.Assert(requestUrl != null);
            var host = requestUrl.Host;
            var port = this._useSecureChannel
                           ? ConfigurationManager.AppSettings["HTTPSPort"]
                           : ConfigurationManager.AppSettings["HTTPPort"];
            if (!string.IsNullOrWhiteSpace(port))
            {
                host += ":" + port;
            }

            var url = UrlHelper.GenerateUrl("",
                                            this._actionName,
                                            this._controllerName,
                                            this._useSecureChannel ? "https" : "http",
                                            host,
                                            null,
                                            this._routeValues,
                                            RouteTable.Routes,
                                            context.RequestContext,
                                            false
                );

            var builder = new UriBuilder(url)
                              {
                                  Query = HttpUtility.ParseQueryString(requestUrl.Query).ToString()
                              };

            context.HttpContext.Response.Redirect(builder.ToString(), false);
        }
    }
}
