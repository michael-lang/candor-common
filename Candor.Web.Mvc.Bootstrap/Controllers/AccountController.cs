using System.Globalization;
using System.Web.Mvc;
using Candor.Configuration.Provider;
using CandorMvcApplication.Models.Account;
using Candor.Security;
using Candor;
using Candor.Web.Mvc;

namespace CandorMvcApplication.Controllers
{
    public partial class AccountController : Controller
    {
        // GET: /Account/Login
        [AllowAnonymous]
        public virtual ActionResult Login(string returnUrl)
        {
            var model = new LoginViewModel();
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = new ExecutionResults();
                var id = UserManager.AuthenticateUser(model.UserName, model.Password,
                    model.RememberMe ? UserSessionDurationType.Extended : UserSessionDurationType.PublicComputer,
                    Request.UserHostAddress, result);
                if (id.IsAuthenticated && result.Success)
                {   //login successful
                    SecurityContextManager.CurrentUser = new UserPrincipal(id);
                    return RedirectToLocal(returnUrl);
                }
                
                //failed business layer validations
                for (var e = result.Messages.Count - 1; e >= 0; e--)
                {
                    ModelState.AddModelError(e.ToString(CultureInfo.InvariantCulture), result.Messages[e].Message);
                }
            }
            // failed data annotation validations
            return View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult LogOff()
        {
            SecurityContextManager.CurrentUser = new UserPrincipal(); //anonymous
            return RedirectToAction(MVC.Home.Index());
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public virtual ActionResult Register()
        {
            var model = new RegisterViewModel();
            model.Load();
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult CheckAliasIsAvailable(string alias)
        {
            if (alias == SecurityContextManager.CurrentIdentity.Name)
                return Json(new { success = true, message = "This is your current sign in alias." });

            var result = new ExecutionResults();
            if (!UserManager.ValidateName(alias, result))
                return Json(new { success = false, message = result.ToHtmlString() });

            var user = UserManager.GetUserByName(alias);
            return Json(new { success = (user == null), message = (User == null) ? "This name is available" : "This name is not available.  Choose another name." });
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Register(RegisterViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var results = new ExecutionResults();
                var user = model.ToUser();
                var identity = UserManager.RegisterUser(user, UserSessionDurationType.Extended, Request.UserHostAddress, results);
                if (results.Success)
                {   //successful registration
                    SecurityContextManager.CurrentUser = new UserPrincipal(identity);
                    return RedirectToLocal(returnUrl);
                }
                //failed business layer
                results.AppendError("Failed to complete registration.");
                for (var e = 0; e < results.Messages.Count; e++)
                {
                    ModelState.AddModelError(e.ToString(CultureInfo.InvariantCulture), results.Messages[e].Message);
                }
            }
            //failed data annotation validations
            model.Load();
            return View(model);
        }

        // GET: /Account/ChangePassword
        public virtual ActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel
            {
                UserName = SecurityContextManager.CurrentIdentity.Name
            };
            return View(model);
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        public virtual ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = new ExecutionResults();
                var user = new User
                               {
                                   UserID = SecurityContextManager.CurrentIdentity.Ticket.UserSession.UserID,
                                   Name = model.UserName,
                                   PasswordHash = model.ConfirmPassword
                               };
                if (UserManager.UpdateUser(user, model.OldPassword, Request.UserHostAddress, result))
                {   //success
                    if (this.IsJsonRequest())
                        return Json(new { success = true });
                    
                    return RedirectToAction(MVC.Account.ChangePasswordSuccess());
                }
                //failed business layer rules
                if (this.IsJsonRequest())
                    return Json(new { success = false, message = result.ToHtmlString() });
                
                for (int e = 0; e < result.Messages.Count; e++)
                {
                    ModelState.AddModelError(e.ToString(CultureInfo.InvariantCulture), result.Messages[e].Message);
                }
                return View(model);
            }
            if (this.IsJsonRequest())
                return Json(new { success = false, errors = ModelState.ToJson() });
            
            return View(model); //modelstate already populated
        }

        // GET: /Account/ChangePasswordSuccess
        public virtual ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        // GET: /Account/ForgotPassword
        public virtual ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public virtual ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = new ExecutionResults();
                ProviderResolver<UserNotificationProvider>.Get.Provider.NotifyPasswordReset(model.UserName, result);
                
                if (this.IsJsonRequest())
                    return Json(new { success = result.Success, message = result.ToHtmlString() });

                if (result.Success)
                    return RedirectToAction(MVC.Account.Login());

                for (var e = 0; e < result.Messages.Count; e++)
                {
                    ModelState.AddModelError(e.ToString(CultureInfo.InvariantCulture), result.Messages[e].Message);
                }
                return View(model);
            }
            if (this.IsJsonRequest())
                return Json(new { success = false, errors = ModelState.ToJson() });
            
            return View(model); //modelstate already populated
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            //redirect to the main landing page for an authenticated user
            return RedirectToAction(MVC.Home.Index());
        }
    }
}