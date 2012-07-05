using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace AirService.Web.Infrastructure
{
    public static class ViewHelper
    {
        public static MvcHtmlString TimeFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                                                               Expression<Func<TModel, TProperty>> expression,
                                                               object htmlAttributes = null)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            string fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var value = (string) GetModelValue(htmlHelper.ViewData, fullName, typeof (string));

            var options = new List<SelectListItem>();
            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    //why are we not keeping ':' char??
                    string timeValue = string.Format("{0:00}{1:00}", i, j*10);
                    options.Add(new SelectListItem
                                    {
                                        Text = string.Format("{0:00}:{1:00}", i, j*10),
                                        Value = timeValue,
                                        Selected = timeValue == value
                                    });
                }
            }

            return htmlHelper.DropDownList(fullName, options, htmlAttributes);
        }


        private static object GetModelValue(ViewDataDictionary viewData, string key, Type destinationType)
        {
            ModelState modelState;
            if (viewData.ModelState.TryGetValue(key, out modelState))
            {
                if (modelState.Value != null)
                {
                    return modelState.Value.ConvertTo(destinationType, null /* culture */);
                }
            }

            return null;
        }

        public static void RenderPartialPrefixed(this HtmlHelper htmlHelper, string partialViewName, object model,
                                                 string prefix)
        {
            htmlHelper.RenderPartial(partialViewName,
                                     model,
                                     new ViewDataDictionary {TemplateInfo = new TemplateInfo {HtmlFieldPrefix = prefix}});
        }

        public static MvcHtmlString PartialPrefixed(this HtmlHelper htmlHelper, string partialViewName, object model,
                                                    string prefix,
                                                    ViewDataDictionary viewData = null)
        {
            viewData = viewData ?? new ViewDataDictionary(model);
            viewData.TemplateInfo = viewData.TemplateInfo ?? new TemplateInfo();
            viewData.TemplateInfo.HtmlFieldPrefix = prefix;
            return htmlHelper.Partial(partialViewName, model, viewData);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper,
                                               string linkText,
                                               string actionName,
                                               string controllerName,
                                               bool useSecureChannel)
        {
            return ActionLink(htmlHelper,
                              linkText,
                              actionName,
                              controllerName,
                              null,
                              null,
                              useSecureChannel);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper,
                                               string linkText,
                                               string actionName,
                                               string controllerName,
                                               object routeValues,
                                               object htmlAttributes,
                                               bool useSecureChannel)
        {
            var httpRequestBase = htmlHelper.ViewContext.HttpContext.Request;
            if (useSecureChannel == httpRequestBase.IsSecureConnection)
            {
                return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);
            }

            // if we are pointing to different channel, write absolute URI
            var host = httpRequestBase.Url.Host;
            var port = useSecureChannel
                           ? ConfigurationManager.AppSettings["HTTPSPort"]
                           : ConfigurationManager.AppSettings["HTTPPort"];
            if (!string.IsNullOrWhiteSpace(port))
            {
                host += ":" + port;
            }

            return htmlHelper.ActionLink(linkText,
                                         actionName,
                                         controllerName,
                                         useSecureChannel ? "https" : "http",
                                         host,
                                         null,
                                         routeValues,
                                         htmlAttributes);
        }
    }
}