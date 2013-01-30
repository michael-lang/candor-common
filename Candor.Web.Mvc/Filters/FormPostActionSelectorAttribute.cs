using System;
using System.Web.Mvc;

namespace Candor.Web.Mvc.Filters
{

    /// <summary>
    /// Matches a controller action method to a post from another action where the attributed action is the name of a button on the form.
    /// </summary>
    /// <remarks>
    /// Variation from source article: http://blog.ashmind.com/2010/03/15/multiple-submit-buttons-with-asp-net-mvc-final-solution/
    /// </remarks>
    public class FormPostActionSelectorAttribute : ActionNameSelectorAttribute
    {
        /// <summary>
        /// Gets or sets the name of the action method that must match.  Or leave empty to match on a combination of the method name and button name.
        /// </summary>
        public string FormActionName { get; set; }
        /// <summary>
        /// Gets or sets the name of the button (form field name) that must post in order to match the action method.
        /// </summary>
        public string ButtonName { get; set; }
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public FormPostActionSelectorAttribute() : base() { }
        /// <summary>
        /// Determines if the current request matches this method.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="actionName"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public override bool IsValidName(ControllerContext controllerContext, string actionName, System.Reflection.MethodInfo methodInfo)
        {
            if (!string.IsNullOrEmpty(ButtonName))
            {
                if (controllerContext.RequestContext.HttpContext.Request[ButtonName] != ButtonName)
                    return false;
                if (!string.IsNullOrEmpty(FormActionName))
                {
                    if (actionName == FormActionName && actionName.Equals(methodInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                        return true;

                    if (methodInfo.Name == ButtonName && actionName == FormActionName)
                        return true;
                    return false;
                }
                if (ButtonName.StartsWith(actionName))
                    return methodInfo.Name == ButtonName || methodInfo.Name == actionName + ButtonName;
                return methodInfo.Name == actionName + ButtonName;
            }
            if (!string.IsNullOrEmpty(FormActionName))
            {
                if (actionName.Equals(methodInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                    return true;

                if (!actionName.Equals(FormActionName, StringComparison.InvariantCultureIgnoreCase))
                    return false;

                return controllerContext.RequestContext.HttpContext.Request[methodInfo.Name] != null;
            }
            return false;
        }
    }
}
