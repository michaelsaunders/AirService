using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace AirService.Web.Helpers
{
    public static class Extensions
    {
        /*
         * Image Link HTML helper
         */

        /// <summary>
        /// return image link
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="imageUrl">URL for image</param>
        /// <param name="controller">target controller name</param>
        /// <param name="action">target action name</param>
        /// <param name="linkText">anchor text</param>
        public static MvcHtmlString ActionImage(this HtmlHelper helper, string imageUrl, string controller, string action, string linkText)
        {
            return ActionImage(helper, null, controller, action, linkText, imageUrl, null, null, null, null);
        }

        /// <summary>
        /// return image link
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="imageUrl">URL for image</param>
        /// <param name="controller">target controller name</param>
        /// <param name="action">target action name</param>
        /// <param name="linkText">anchor text</param>
        /// <param name="htmlAttributes">anchor attributes</param>
        public static MvcHtmlString ActionImage(this HtmlHelper helper, string imageUrl, string controller, string action, string linkText, object routeValues)
        {
            return ActionImage(helper, null, controller, action, linkText, imageUrl, null, null, new RouteValueDictionary(routeValues), null);
        }

        /// <summary>
        /// return image link
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="imageUrl">URL for image</param>
        /// <param name="controller">target controller name</param>
        /// <param name="action">target action name</param>
        /// <param name="linkText">anchor text</param>
        /// <param name="htmlAttributes">anchor attributes</param>
        /// <param name="routeValues">route values</param>
        public static MvcHtmlString ActionImage(this HtmlHelper helper, string imageUrl, string controller, string action, string linkText, object routeValues, object htmlAttributes)
        {
            return ActionImage(helper, null, controller, action, linkText, imageUrl, null, null, new RouteValueDictionary(routeValues), htmlAttributes);
        }

        /// <summary>
        /// return image link
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="id">Id of link control</param>
        /// <param name="controller">target controller name</param>
        /// <param name="action">target action name</param>
        /// <param name="linkText">The link text.</param>
        /// <param name="strImageUrl">URL for image</param>
        /// <param name="alternateText">Alternate Text for the image</param>
        /// <param name="strStyle">style of the image like border properties, etc</param>
        /// <returns></returns>
        public static MvcHtmlString ActionImage(this HtmlHelper helper, string id, string controller, string action, string linkText, string strImageUrl, string alternateText, string strStyle)
        {
            return ActionImage(helper, id, controller, action, linkText, strImageUrl, alternateText, strStyle, null, null);
        }

        /// <summary>
        /// return image link
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="id">Id of link control</param>
        /// <param name="controller">target controller name</param>
        /// <param name="action">target action name</param>
        /// <param name="linkText">anchor text</param>
        /// <param name="strImageUrl">URL for image</param>
        /// <param name="alternateText">Alternate Text for the image</param>
        /// <param name="strStyle">style of the image like border properties, etc</param>
        /// <param name="routeValues">The route values.</param>
        /// <param name="htmlAttributes">html attribues for link</param>
        /// <returns></returns>
        public static MvcHtmlString ActionImage(this HtmlHelper helper, string id, string controller, string action, string linkText, string strImageUrl, string alternateText, string strStyle, RouteValueDictionary routeValues, object htmlAttributes)
        {

            //    var url = new UrlHelper(html.ViewContext.RequestContext);

            //    // build the <img> tag
            //    var imgBuilder = new TagBuilder("img");
            //    imgBuilder.MergeAttribute("src", url.Content(imagePath));
            //    imgBuilder.MergeAttribute("alt", alt);
            //    string imgHtml = imgBuilder.ToString(TagRenderMode.SelfClosing);

            //    // build the <a> tag
            //    var anchorBuilder = new TagBuilder("a");
            //    anchorBuilder.MergeAttribute("href", url.Action(action, controllerName, routeValues));
            //    anchorBuilder.InnerHtml = imgHtml; // include the <img> tag inside
            //    string anchorHtml = anchorBuilder.ToString(TagRenderMode.Normal);

            //    return MvcHtmlString.Create(anchorHtml);

            // Build the img tag

            TagBuilder image = new TagBuilder("img");
            image.MergeAttribute("src", new UrlHelper(helper.ViewContext.RequestContext).Content(strImageUrl));
            image.MergeAttribute("alt", alternateText);
            image.MergeAttribute("valign", "middle");
            image.MergeAttribute("border", "none");
            //if (htmlAttributes != null)
            //    image.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            TagBuilder span = new TagBuilder("span");

            // Create tag builder
            var anchor = new TagBuilder("a");
            var url = new UrlHelper(helper.ViewContext.RequestContext).Action(action, controller, routeValues);

            // Create valid id
            anchor.GenerateId(id);

            // Add attributes
            anchor.MergeAttribute("href", url);
            //anchor.MergeAttribute("class", "actionImage");

            if (htmlAttributes != null)
            {
                anchor.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            }

            // place the img tag inside the anchor tag.)
            if (String.IsNullOrEmpty(linkText))
            {
                anchor.InnerHtml = image.ToString(TagRenderMode.Normal);
            }
            else
            {
                span.InnerHtml = linkText;
                anchor.InnerHtml = image.ToString(TagRenderMode.Normal) + " " + span.ToString(TagRenderMode.Normal);
            }

            // Render tag
            string anchorHtml = anchor.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(anchorHtml);
        }

        /// <summary>
        /// Return a new script date object
        /// </summary>
        public static MvcHtmlString ScriptDateTime(this HtmlHelper helper, DateTime dateTime)
        {
            return MvcHtmlString.Create(string.Format("new Date({0},{1},{2},{3},{4},{5})",
                                                      dateTime.Year,
                                                      (dateTime.Month - 1),
                                                      dateTime.Day,
                                                      dateTime.Hour,
                                                      dateTime.Minute,
                                                      dateTime.Second));
        }
        

        #region IEnumerable<> to SelectList<>
        /// <summary>
        /// Converts the source sequence into an IEnumerable of SelectListItem
        /// </summary>
        /// <param name="items">Source sequence</param>
        /// <param name="nameSelector">Lambda that specifies the name for the SelectList items</param>
        /// <param name="valueSelector">Lambda that specifies the value for the SelectList items</param>
        /// <returns>IEnumerable of SelectListItem</returns>
        public static IEnumerable<SelectListItem> ToSelectList<TItem, TValue>(this IEnumerable<TItem> items, Func<TItem, TValue> valueSelector, Func<TItem, string> nameSelector)
        {
            return items.ToSelectList(valueSelector, nameSelector, x => false);
        }

        /// <summary>
        /// Converts the source sequence into an IEnumerable of SelectListItem
        /// </summary>
        /// <param name="items">Source sequence</param>
        /// <param name="nameSelector">Lambda that specifies the name for the SelectList items</param>
        /// <param name="valueSelector">Lambda that specifies the value for the SelectList items</param>
        /// <param name="selectedItems">Those items that should be selected</param>
        /// <returns>IEnumerable of SelectListItem</returns>
        public static IEnumerable<SelectListItem> ToSelectList<TItem, TValue>(this IEnumerable<TItem> items, Func<TItem, TValue> valueSelector, Func<TItem, string> nameSelector, IEnumerable<TValue> selectedItems)
        {
            return items.ToSelectList(valueSelector, nameSelector, x => selectedItems != null && selectedItems.Contains(valueSelector(x)));
        }

        /// <summary>
        /// Converts the source sequence into an IEnumerable of SelectListItem
        /// </summary>
        /// <param name="items">Source sequence</param>
        /// <param name="nameSelector">Lambda that specifies the name for the SelectList items</param>
        /// <param name="valueSelector">Lambda that specifies the value for the SelectList items</param>
        /// <param name="selectedValueSelector">Lambda that specifies whether the item should be selected</param>
        /// <returns>IEnumerable of SelectListItem</returns>
        public static IEnumerable<SelectListItem> ToSelectList<TItem, TValue>(this IEnumerable<TItem> items, Func<TItem, TValue> valueSelector, Func<TItem, string> nameSelector, Func<TItem, bool> selectedValueSelector)
        {
            foreach (var item in items)
            {
                var value = valueSelector(item);

                yield return new SelectListItem
                {
                    Text = nameSelector(item),
                    Value = value.ToString(),
                    Selected = selectedValueSelector(item)
                };
            }
        }

        #endregion
    }

}