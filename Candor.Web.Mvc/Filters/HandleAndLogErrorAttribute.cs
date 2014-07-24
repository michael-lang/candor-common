using Common.Logging;
using System.Web.Mvc;

namespace Candor.Web.Mvc.Filters
{
    /// <summary>
    /// An MVC filter attribute to log unhandled exceptions.
    /// </summary>
    public class HandleAndLogErrorAttribute : HandleErrorAttribute
    {
        private ILog LogProvider = LogManager.GetLogger(typeof(HandleAndLogErrorAttribute));

        /// <summary>
        /// Called when an exception occurs.
        /// </summary>
        /// <param name="filterContext">The action-filter context.</param><exception cref="T:System.ArgumentNullException">The <paramref name="filterContext"/> parameter is null.</exception>
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception != null)
                LogProvider.Error(filterContext.Exception.Message, filterContext.Exception);
            else
                LogProvider.Error("Unknown Error.");

            base.OnException(filterContext);
        }
    }
}
