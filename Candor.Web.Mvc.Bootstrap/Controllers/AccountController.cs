using System;
using System.Globalization;
using System.Web.Mvc;
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
            LoginViewModel model = new LoginViewModel();
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                var result = new ExecutionResults();
                UserIdentity id = UserManager.AuthenticateUser(model.UserName, model.Password,
                    model.RememberMe ? UserSessionDurationType.Extended : UserSessionDurationType.PublicComputer,
                    this.Request.UserHostAddress, result);
                if (id.IsAuthenticated && result.Success)
                {   //login successful
                    SecurityContextManager.CurrentUser = new UserPrincipal(id);
                    return RedirectToLocal(returnUrl);
                }
                else
                {   //failed business layer validations
                    for (int e = result.Messages.Count - 1; e >= 0; e--)
                    {
                        this.ModelState.AddModelError(e.ToString(CultureInfo.InvariantCulture), result.Messages[e].Message);
                    }
                }
            }
            // failed data annotation validations
            return this.View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult LogOff()
        {
            SecurityContextManager.CurrentUser = new UserPrincipal(); //anonymous
            return this.RedirectToAction(MVC.Home.Index());
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public virtual ActionResult Register()
        {
            var model = new RegisterViewModel();
            model.Load();
            return this.View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult CheckAliasIsAvailable(string alias)
        {
            if (alias == SecurityContextManager.CurrentIdentity.Name)
                return Json(new { success = true, message = "This is your current sign in alias." });

            var result = new ExecutionResults();
            if (!UserManager.ValidateName(alias, result))
                return Json(new { success = false, message = result.ToHtmlString() });

            User user = UserManager.GetUserByName(alias);
            return Json(new { success = (user == null), message = (User == null) ? "This name is available" : "This name is not available.  Choose another name." });
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Register(RegisterViewModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
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
                for (int e = 0; e < results.Messages.Count; e++)
                {
                    this.ModelState.AddModelError(e.ToString(CultureInfo.InvariantCulture), results.Messages[e].Message);
                }
            }
            //failed data annotation validations
            model.Load();
            return this.View(model);
        }

        // GET: /Account/ChangePassword
        public virtual ActionResult ChangePassword()
        {
            ChangePasswordViewModel model = new ChangePasswordViewModel();
            model.UserName = SecurityContextManager.CurrentIdentity.Name;
            return this.View(model);
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        public virtual ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var result = new ExecutionResults();
                var user = new User()
                               {
                                   UserID = SecurityContextManager.CurrentIdentity.Ticket.UserSession.UserID,
                                   Name = model.UserName,
                                   PasswordHash = model.ConfirmPassword
                               };
                if (UserManager.UpdateUser(user, model.OldPassword, Request.UserHostAddress, result))
                {   //success
                    if (this.IsJsonRequest())
                        return Json(new { success = true });
                    else
                        return this.RedirectToAction(MVC.Account.ChangePasswordSuccess());
                }
                //failed business layer rules
                if (this.IsJsonRequest())
                    return Json(new { success = false, message = result.ToHtmlString() });
                else
                {
                    for (int e = 0; e < result.Messages.Count; e++)
                    {
                        this.ModelState.AddModelError(e.ToString(CultureInfo.InvariantCulture), result.Messages[e].Message);
                    }
                    return this.View(model);
                }
            }
            if (this.IsJsonRequest())
                return Json(new { success = false, errors = this.ModelState.ToJson() });
            else
                return this.View(model); //modelstate already populated
        }

        // GET: /Account/ChangePasswordSuccess
        public virtual ActionResult ChangePasswordSuccess()
        {
            return this.View();
        }

        // GET: /Account/ForgotPassword
        public virtual ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public virtual ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var result = new ExecutionResults();
                var resetCode = UserManager.GenerateUserResetCode(model.UserName);
                if (!String.IsNullOrWhiteSpace(resetCode)) //user found
                {
                    //TODO: generate email body with reset code.
                }
                else
                {
                    //TODO: generate alternate body telling how to sign up.
                }
#warning TODO: send the reset code via email (or signup instructions if account does not exist) and then show a view with email confirmation.
                //TODO: send the email (whether user found or not!)
                //TODO: return an ActionResult if successful

                result.AppendError("The forgot password functionality is not yet implemented.");
                
                //failed business layer rules
                if (this.IsJsonRequest())
                    return Json(new { success = false, message = result.ToHtmlString() });
                else
                {
                    for (int e = 0; e < result.Messages.Count; e++)
                    {
                        this.ModelState.AddModelError(e.ToString(CultureInfo.InvariantCulture), result.Messages[e].Message);
                    }
                    return this.View(model);
                }
            }
            if (this.IsJsonRequest())
                return Json(new { success = false, errors = this.ModelState.ToJson() });
            else
                return this.View(model); //modelstate already populated
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