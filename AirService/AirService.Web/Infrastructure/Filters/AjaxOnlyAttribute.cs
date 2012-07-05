using System.Web.Mvc;

namespace AirService.Web.Infrastructure.Filters
{
    public class AjaxOnlyAttribute : ActionFilterAttribute
    {
        public string RedirectAction { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var controller = filterContext.Controller.ViewData["controller"];
                var action = RedirectAction ?? "Index";

                filterContext.Result =
                    new RedirectResult(new UrlHelper(filterContext.RequestContext).Action(action, controller));
            }

            base.OnActionExecuting(filterContext);
        }
    }
}