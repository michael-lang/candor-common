using System;
using System.Linq;
using System.Web;

namespace Candor.Security.Web
{
	/// <summary>
	/// Authenticates the current request from custom cookies.
	/// </summary>
	public class CandorAuthenticationModule : IHttpModule
	{
		private static readonly string ImpersonationKey = typeof(CandorAuthenticationModule).FullName + "-ImpersonatedUser";
		private const string AuthenticationTicketTokenKey = "AuthorizationTicketToken";
		private const string RememberMeKey = "RememberMe";
		private static readonly string[] IgnoreAuthExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif" };

		private static bool ImpersonationEnabled { get; set; }

		#region IHttpModule interface members
		/// <summary>
		/// Initializes this instance from the application.
		/// </summary>
		/// <param name="app"></param>
		public void Init( HttpApplication app )
		{
			app.AuthenticateRequest += OnAuthenticate;
			//app.PostAuthenticateRequest += OnPostAuthenticateRequest;
			//app.BeginRequest += BeginRequest;
			app.EndRequest += OnEndRequest;

			string sImpersonationEnabled = System.Configuration.ConfigurationManager.AppSettings["EnableApplicationImpersonation"];
			ImpersonationEnabled = !string.IsNullOrEmpty(sImpersonationEnabled) && (sImpersonationEnabled.ToUpper() == "TRUE" || sImpersonationEnabled.ToUpper() == "YES");
		}
		/// <summary>
		/// Disposes of this instance.
		/// </summary>
		public void Dispose() { }
		#endregion IHttpModule interface members

		/// <summary>
		/// Checks if a request needs to be authentication based on the url requested.  Static resource files do not require authentication.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private static bool CheckRequireAuthentication( HttpContext context )
		{
			var extension = System.IO.Path.GetExtension(context.Request.Url.LocalPath);
			if (extension != null)
			{
				if (IgnoreAuthExtensions.Contains(extension.ToLower()))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Handles the OnAuthenticate event subscribed to during <see cref="Init"/>.
		/// </summary>
		/// <param name="sender">The HttpApplication.</param>
		/// <param name="e">Not used.</param>
		private void OnAuthenticate( object sender, EventArgs e )
		{
			var app = (HttpApplication)sender;
			OnAuthenticate(app.Context);
		}
/*
		void OnPostAuthenticateRequest( object sender, EventArgs e )
		{
			//var app = (HttpApplication)sender;
			//OnAuthenticate(app.Context);
		}
*/
		/// <summary>
		/// Authenticates the current context.
		/// </summary>
		/// <param name="context">The context containing the current request to be authenticated and the response.</param>
		public static void OnAuthenticate( HttpContext context )
		{
			if (!CheckRequireAuthentication(context))
				return;

			var ticketCookie = HttpContext.Current.Request.Cookies[AuthenticationTicketTokenKey];
            var ticketHeader = HttpContext.Current.Request.Headers["X-" + AuthenticationTicketTokenKey];
			var rememberCookie = HttpContext.Current.Request.Cookies[RememberMeKey];
		    var rememberHeader = HttpContext.Current.Request.Headers["X-" + RememberMeKey];
			var rememberMe = false;
			if (rememberCookie != null && !String.IsNullOrWhiteSpace(rememberCookie.Value))
				Boolean.TryParse(rememberCookie.Value, out rememberMe);
            else if (!String.IsNullOrWhiteSpace(rememberHeader))
                Boolean.TryParse(rememberHeader, out rememberMe);
			var ipAddress = context.Request.UserHostAddress;

			if ((ticketCookie == null || string.IsNullOrWhiteSpace(ticketCookie.Value))
                && string.IsNullOrWhiteSpace(ticketHeader))
			{
				SecurityContextManager.CurrentUser = new UserPrincipal(); //anonymous
				return;
			}

			var identity = UserManager.AuthenticateUser(ticketHeader ?? ticketCookie.Value, rememberMe ? UserSessionDurationType.Extended : UserSessionDurationType.PublicComputer, ipAddress, new ExecutionResults());
			var principal = new UserPrincipal(identity);
			SecurityContextManager.CurrentUser = principal;

			if (ImpersonationEnabled && !principal.IsAnonymous && principal.IsInAnyRole(UserManager.Provider.ImpersonationAllowedRoles))
			{	//check for impersonation
				HttpCookie impersonatedUserCookie = context.Request.Cookies[ImpersonationKey];
			    var impersonatedHeader = context.Request.Headers["X-" + ImpersonationKey];
				if (!String.IsNullOrWhiteSpace(impersonatedHeader) ||
                    (impersonatedUserCookie != null && !string.IsNullOrEmpty(impersonatedUserCookie.Value)))
				{
                    var impersonatedUser = UserManager.GetUserByName(impersonatedHeader ?? impersonatedUserCookie.Value);
                    if (impersonatedUser != null)
                    {
                        principal = new UserPrincipal(new UserIdentity(impersonatedUser.UserID, impersonatedUser.Name, identity));
                        SecurityContextManager.CurrentUser = principal;
                    }
				}
			}
		}

		//private void BeginRequest(object sender, EventArgs e)
		//{	//just a test
		//    if (SecurityContextManager.IsAnonymous)
		//    {
		//        OnAuthenticate(sender, e);
		//    }
		//}
		private void OnEndRequest( object sender, EventArgs e )
		{
			var app = (HttpApplication)sender;
			if (!CheckRequireAuthentication(app.Context))
				return;

			if (ImpersonationEnabled)
			{
				HttpCookie cookie = app.Context.Response.Cookies[ImpersonationKey];
				if (SecurityContextManager.IsImpersonating)
				{
					if (cookie == null)
					{
						cookie = new HttpCookie(ImpersonationKey) {Secure = true};
						app.Context.Response.Cookies.Add(cookie);
					}
					cookie.Expires = DateTime.Now.AddMinutes(15);
                    cookie.Value = SecurityContextManager.CurrentUser.Identity.Name;
				}
				else if (cookie != null)
				{
					cookie.Expires = DateTime.Now.AddDays(-1);
					app.Context.Response.Cookies.Remove(ImpersonationKey);
				}
			}
			HttpCookie authCookie = app.Context.Response.Cookies[AuthenticationTicketTokenKey];
			if (authCookie == null)
			{
				authCookie = new HttpCookie(AuthenticationTicketTokenKey) {Secure = true};
				app.Context.Response.Cookies.Add(authCookie);
			}
			if (!SecurityContextManager.IsAnonymous)
			{
				authCookie.Expires = SecurityContextManager.CurrentUser.Identity.Ticket.UserSession.ExpirationDate;
				authCookie.Value = SecurityContextManager.CurrentUser.Identity.Ticket.UserSession.RenewalToken.ToString();

				HttpCookie rememberCookie = app.Context.Response.Cookies[RememberMeKey];
				if (rememberCookie == null)
				{
					rememberCookie = new HttpCookie(RememberMeKey) {Secure = true};
					app.Context.Response.Cookies.Add(rememberCookie);
				}
				rememberCookie.Expires = authCookie.Expires;
			    bool remember = ((authCookie.Expires - DateTime.UtcNow).TotalMinutes > 21);
				rememberCookie.Value = remember.ToString();
			}
		}
	}
}