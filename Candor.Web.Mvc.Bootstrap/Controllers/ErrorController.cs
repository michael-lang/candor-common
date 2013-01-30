using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common.Logging;

namespace CandorMvcApplication.Controllers
{
    public partial class ErrorController : Controller
    {
        private ILog LogProvider = LogManager.GetLogger(typeof(ErrorController));

        // GET: /Error/
        public virtual ActionResult Index()
        {
            return this.View(MVC.Shared.Views.Error);
        }

        public virtual ActionResult NotFound()
        {
            LogProvider.WarnFormat("NotFound:{0}", this.Request.RawUrl);
            //source: http://www.devcurry.com/2012/06/aspnet-mvc-handling-exceptions-and-404.html
            this.Response.StatusCode = 404;
            this.Response.TrySkipIisCustomErrors = true;
            return this.View();
        }
    }
}
