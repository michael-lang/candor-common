using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Candor.Web.Mvc.Filters
{
    public class HandleAndLogErrorAttribute : HandleErrorAttribute
    {
        private ILog LogProvider = LogManager.GetLogger(typeof(HandleAndLogErrorAttribute));

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
