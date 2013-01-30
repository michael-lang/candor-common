using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using System.Web.Routing;

namespace Candor.Web.Mvc
{
    /// <summary>
    /// Various extensions to the HtmlHelper class.
    /// </summary>
    public static class HtmlHelperExtension
    {
        /// <summary>
        /// Generates an html image tag using the specified image fileName and any other html attributes to be applied to the image tag.
        /// </summary>
        /// <param name="helper">The class being extended with this method.</param>
        /// <param name="fileName">The file name of the image including full relative path.  Recommend coming from T4MVC Links property.</param>
        public static MvcHtmlString ImageTag(this HtmlHelper helper, string fileName) { return helper.ImageTag(fileName, null); }
        /// <summary>
        /// Generates an html image tag using the specified image fileName and any other html attributes to be applied to the image tag.
        /// </summary>
        /// <param name="helper">The class being extended with this method.</param>
        /// <param name="fileName">The file name of the image including full relative path.  Recommend coming from T4MVC Links property.</param>
        /// <param name="htmlAttributes">A dynamic object with name and value pairs.  Example:  new {data-custom1="abc", @class="large"}</param>
        public static MvcHtmlString ImageTag(this HtmlHelper helper, string fileName, object htmlAttributes)
        {
            var url = new UrlHelper(helper.ViewContext.RequestContext);

            // build the <img> tag
            var imgBuilder = new TagBuilder("img");
            imgBuilder.MergeAttribute("src", fileName);
            if (htmlAttributes != null)
                imgBuilder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            string imgHtml = imgBuilder.ToString(TagRenderMode.SelfClosing);

            return MvcHtmlString.Create(imgHtml);
        }
        /// <summary>
        /// Generates an anchor tag (link) with an image inside such that the image when clicked follows the specified MVC route.
        /// </summary>
        /// <param name="helper">The class being extended with this method.</param>
        /// <param name="action">The action method to navigate to when the image link is clicked.</param>
        /// <param name="controllerName">The controller containing the action method to navigate to when the image link is clicked.</param>
        /// <param name="imageFileName">The file name of the image including full relative path.  Recommend coming from T4MVC Links property.</param>
        public static MvcHtmlString ActionImageTag(this HtmlHelper helper, string action, string controllerName, string imageFileName)
        {
            return helper.ActionImageTag(action, controllerName, imageFileName, null, null, null);
        }
        /// <summary>
        /// Generates an anchor tag (link) with an image inside such that the image when clicked follows the specified MVC route.
        /// </summary>
        /// <param name="helper">The class being extended with this method.</param>
        /// <param name="action">The action method to navigate to when the image link is clicked.</param>
        /// <param name="controllerName">The controller containing the action method to navigate to when the image link is clicked.</param>
        /// <param name="imageFileName">The file name of the image including full relative path.  Recommend coming from T4MVC Links property.</param>
        /// <param name="routeValues">Any extra route values required by the action method. Example:  new {id=5}</param>
        public static MvcHtmlString ActionImageTag(this HtmlHelper helper, string action, string controllerName, string imageFileName, object routeValues)
        {
            return helper.ActionImageTag(action, controllerName, imageFileName, routeValues, null, null);
        }
        /// <summary>
        /// Generates an anchor tag (link) with an image inside such that the image when clicked follows the specified MVC route.
        /// </summary>
        /// <param name="helper">The class being extended with this method.</param>
        /// <param name="result">An IT4MVCActionResult from a T4 MVC action method used to help build urls.</param>
        /// <param name="imageFileName">The file name of the image including full relative path.  Recommend coming from T4MVC Links property.</param>
        /// <param name="anchorHtmlAttributes">A dynamic object with name and value pairs for the anchor tag.  Example:  new {data-custom1="abc", @class="large"}</param>
        /// <param name="imageHtmlAttributes">A dynamic object with name and value pairs for the image tag.  Example:  new {data-custom1="abc", @class="large"}</param>
        /// <returns></returns>
        public static MvcHtmlString ActionImageTag(this HtmlHelper helper, ActionResult result, string imageFileName, object anchorHtmlAttributes, object imageHtmlAttributes)
        {
            IT4MVCActionResult t4 = result.GetT4MVCResult();
            return ActionImageTag(helper, t4.Action, t4.Controller, imageFileName, t4.RouteValueDictionary, anchorHtmlAttributes, imageHtmlAttributes);
        }
        /// <summary>
        /// Generates an anchor tag (link) with an image inside such that the image when clicked follows the specified MVC route.
        /// </summary>
        /// <param name="helper">The class being extended with this method.</param>
        /// <param name="action">The action method to navigate to when the image link is clicked.</param>
        /// <param name="controllerName">The controller containing the action method to navigate to when the image link is clicked.</param>
        /// <param name="imageFileName">The file name of the image including full relative path.  Recommend coming from T4MVC Links property.</param>
        /// <param name="routeValues">Any extra route values required by the action method. Example:  new {id=5}</param>
        /// <param name="anchorHtmlAttributes">A dynamic object with name and value pairs for the anchor tag.  Example:  new {data-custom1="abc", @class="large"}</param>
        /// <param name="imageHtmlAttributes">A dynamic object with name and value pairs for the image tag.  Example:  new {data-custom1="abc", @class="large"}</param>
        public static MvcHtmlString ActionImageTag(this HtmlHelper helper, string action, string controllerName, string imageFileName, object routeValues, object anchorHtmlAttributes, object imageHtmlAttributes)
        {
            var url = new UrlHelper(helper.ViewContext.RequestContext);

            // build the <img> tag
            var imgBuilder = new TagBuilder("img");
            imgBuilder.MergeAttribute("src", imageFileName);
            if (imageHtmlAttributes != null)
                imgBuilder.MergeAttributes(new RouteValueDictionary(imageHtmlAttributes));
            string imgHtml = imgBuilder.ToString(TagRenderMode.SelfClosing);

            // build the <a> tag
            var anchorBuilder = new TagBuilder("a");
            if (routeValues != null)
                anchorBuilder.MergeAttribute("href", url.Action(action, controllerName, routeValues));
            else
                anchorBuilder.MergeAttribute("href", url.Action(action, controllerName));
            if (anchorHtmlAttributes != null)
                anchorBuilder.MergeAttributes(new RouteValueDictionary(anchorHtmlAttributes));
            anchorBuilder.InnerHtml = imgHtml; // include the <img> tag inside
            string anchorHtml = anchorBuilder.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(anchorHtml);
        }
        /// <summary>
        /// An enhancement to the MVC3 same named method that takes into account other dictionary types not being double decoded.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="htmlAttributes">An anonymous object, RouteValueDictionary, or IDictionary of string,object.</param>
        /// <returns></returns>
        public static RouteValueDictionary AnonymousObjectToHtmlAttributesMvc2(this HtmlHelper htmlHelper, object htmlAttributes)
        {	//originally copied from MVC3 source
            if (htmlAttributes is RouteValueDictionary)
                return (RouteValueDictionary)htmlAttributes; //<- this is not in the base MVC3 version - Too bad.

            if (htmlAttributes is IDictionary<string, object>)
                return new RouteValueDictionary((IDictionary<string, object>)htmlAttributes); //<- this is not in the base MVC3 version - Too bad.

            var result = new RouteValueDictionary();
            if (htmlAttributes != null)
            {
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(htmlAttributes))
                {
                    result.Add(property.Name.Replace('_', '-'), property.GetValue(htmlAttributes));
                }
            }
            return result;
        }
    }
}
