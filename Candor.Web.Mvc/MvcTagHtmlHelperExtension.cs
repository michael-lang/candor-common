using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using System.Web.Routing;

namespace Candor.Web.Mvc
{
    public static class MvcTagHtmlHelperExtension
    {
        /// <summary>
        /// Begins a new action link.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <returns></returns>
        public static MvcTag BeginActionLink(this HtmlHelper htmlHelper, string actionName)
        {
            return BeginActionLink(htmlHelper, actionName, null /* controllerName */, new RouteValueDictionary(), new RouteValueDictionary());
        }
        /// <summary>
        /// Begins a new action link.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <returns></returns>
        public static MvcTag BeginActionLink(this HtmlHelper htmlHelper, string actionName, object routeValues)
        {
            return BeginActionLink(htmlHelper, actionName, null /* controllerName */, new RouteValueDictionary(routeValues), new RouteValueDictionary());
        }
        /// <summary>
        /// Begins a new action link.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <returns></returns>
        public static MvcTag BeginActionLink(this HtmlHelper htmlHelper, string actionName, object routeValues, object htmlAttributes)
        {
            return BeginActionLink(htmlHelper, actionName, null /* controllerName */, new RouteValueDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }
        /// <summary>
        /// Begins a new action link.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <returns></returns>
        public static MvcTag BeginActionLink(this HtmlHelper htmlHelper, string actionName, RouteValueDictionary routeValues)
        {
            return BeginActionLink(htmlHelper, actionName, null /* controllerName */, routeValues, new RouteValueDictionary());
        }
        /// <summary>
        /// Begins a new action link.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <returns></returns>
        public static MvcTag BeginActionLink(this HtmlHelper htmlHelper, string actionName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            return BeginActionLink(htmlHelper, actionName, null /* controllerName */, routeValues, htmlAttributes);
        }
        /// <summary>
        /// Begins a new action link.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <returns></returns>
        public static MvcTag BeginActionLink(this HtmlHelper htmlHelper, string actionName, string controllerName)
        {
            return BeginActionLink(htmlHelper, actionName, controllerName, new RouteValueDictionary(), new RouteValueDictionary());
        }
        /// <summary>
        /// Begins a new action link.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <param name="htmlHelper">The class being extended with this method.</param>
        /// <param name="result">An IT4MVCActionResult from a T4 MVC action method used to help build urls.</param>
        /// <param name="htmlAttributes">A dynamic object with name and value pairs.  Example:  new {data-custom1="abc", @class="large"}</param>
        /// <returns></returns>
        public static MvcTag BeginActionLink(this HtmlHelper htmlHelper, ActionResult result, object htmlAttributes)
        {
            var callInfo = result.GetT4MVCResult();
            return htmlHelper.BeginActionLink(callInfo.Action, callInfo.Controller, callInfo.RouteValueDictionary, htmlAttributes);
        }
        /// <summary>
        /// Begins a new action link.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <param name="htmlHelper">The class being extended with this method.</param>
        /// <param name="result">An IT4MVCActionResult from a T4 MVC action method used to help build urls.</param>
        /// <param name="htmlAttributes">A dictionary with name and value pairs.</param>
        /// <returns></returns>
        public static MvcTag BeginActionLink(this HtmlHelper htmlHelper, ActionResult result, IDictionary<string, object> htmlAttributes)
        {
            var callInfo = result.GetT4MVCResult();
            return htmlHelper.BeginActionLink(callInfo.Action, callInfo.Controller, callInfo.RouteValueDictionary, htmlAttributes);
        }
        /// <summary>
        /// Begins a new action link.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <returns></returns>
        public static MvcTag BeginActionLink(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues, object htmlAttributes)
        {
            string hrefUrl = UrlHelper.GenerateUrl(null, actionName, controllerName, htmlHelper.AnonymousObjectToHtmlAttributesMvc2(routeValues), htmlHelper.RouteCollection, htmlHelper.ViewContext.RequestContext, true);
            //This uses MVC2 helper below instead of MVC3 HtmlHelper.AnonymousObjectToHtmlAttributes because it is not as defensive...
            RouteValueDictionary attrs = htmlHelper.AnonymousObjectToHtmlAttributesMvc2(htmlAttributes);
            attrs.Add("href", hrefUrl);
            return BeginTag(htmlHelper, "a", attrs);
        }
        /// <summary>
        /// Begins a new action link.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <returns></returns>
        public static MvcTag BeginActionLink(this HtmlHelper htmlHelper, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            string hrefUrl = UrlHelper.GenerateUrl(null, actionName, controllerName, routeValues, htmlHelper.RouteCollection, htmlHelper.ViewContext.RequestContext, true);
            //This uses MVC2 helper below instead of MVC3 HtmlHelper.AnonymousObjectToHtmlAttributes because it is not as defensive...
            RouteValueDictionary attrs = htmlHelper.AnonymousObjectToHtmlAttributesMvc2(htmlAttributes);
            attrs.Add("href", hrefUrl);
            return BeginTag(htmlHelper, "a", attrs);
        }
        /// <summary>
        /// Begins a new tag of any kind.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="tagName"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcTag BeginTag(this HtmlHelper htmlHelper, string tagName, object htmlAttributes)
        {
            return htmlHelper.BeginTag(tagName, htmlHelper.AnonymousObjectToHtmlAttributesMvc2(htmlAttributes));
            //This uses MVC2 helper below instead of MVC3 HtmlHelper.AnonymousObjectToHtmlAttributes because it is not as defensive...
        }
        /// <summary>
        /// Begins a new tag of any kind.  For use in a using statement, allowing other code inside the tag.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="tagName"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcTag BeginTag(this HtmlHelper htmlHelper, string tagName, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder(tagName);
            tagBuilder.MergeAttributes(htmlAttributes);

            htmlHelper.ViewContext.Writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));
            MvcTag theTag = new MvcTag(htmlHelper.ViewContext, tagName);

            return theTag;
        }
    }
}
