using System.IO;
using System.Web.Mvc;

namespace Candor.Web.Mvc
{
    /// <summary>
    /// Controller extension capabilities.
    /// </summary>
    public static class ControllerExtension
    {
        /// <summary>
        /// Determines whether a controller action's HTTP request accepts/wants an AJAX response.
        /// </summary>
        public static bool IsJsonRequest(this Controller controller)
        {
            if (controller.Request.AcceptTypes != null)
                foreach (string a in controller.Request.AcceptTypes)
                    if (a.ToLower().Contains("json"))
                        return true;
            return false;
        }
        /// <summary>
        /// Determines whether a controller action's HTTP request is an AJAX request.
        /// </summary>
        public static bool IsAjaxRequest(this Controller controller)
        {
            return controller.Request.IsAjaxRequest();
        }
        /// <summary>
        /// Renders a (partial) view to string accessed based on the folder containing views for the specified controller.
        /// </summary>
        /// <param name="controller">Controller to extend</param>
        /// <param name="viewName">The view name.  Do not supply a full view path (such as with MVC.[ControllerName].Views.[YourViewName]).  This must be just the name of the view file without the extension.</param>
        /// <returns>Rendered (partial) view as string</returns>
        public static string RenderPartialViewToString(this Controller controller, string viewName)
        {
            return controller.RenderPartialViewToString(viewName, null);
        }
        /// <summary>
        /// Renders a (partial) view to string accessed based on the folder containing views for the specified controller.
        /// </summary>
        /// <param name="controller">Controller to extend</param>
        /// <param name="viewName">The view name.  Do not supply a full view path (such as with MVC.[ControllerName].Views.[YourViewName]).  This must be just the name of the view file without the extension.</param>
        /// <param name="model">Model</param>
        /// <returns>Rendered (partial) view as string</returns>
        /// <remarks>
        /// Based on http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/
        /// </remarks>
        public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
