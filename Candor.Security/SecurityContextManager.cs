using System;
using Common.Logging;
using prov = Candor.Configuration.Provider;

namespace Candor.Security
{
	/// <summary>
	/// Access to the current user of the application in an application type neutral way.
	/// </summary>
	public static class SecurityContextManager
	{
		private static prov.ProviderCollection<SecurityContextProvider> _providers;
		private static ILog _logProvider;

		#region Properties
		/// <summary>
		/// Gets or sets the log destination for this type.  If not set, it will be automatically loaded when needed.
		/// </summary>
		public static ILog LogProvider
		{
			get { return _logProvider ?? (_logProvider = LogManager.GetLogger(typeof (SecurityContextManager))); }
			set { _logProvider = value; }
		}
		/// <summary>
		/// Gets the default provider instance.
		/// </summary>
		public static SecurityContextProvider Provider
		{
			get { return Providers.ActiveProvider; }
		}
		/// <summary>
		/// Gets all the configured SecurityContext providers.
		/// </summary>
		public static prov.ProviderCollection<SecurityContextProvider> Providers
		{
			get
			{
				if (_providers == null)
					_providers = new prov.ProviderCollection<SecurityContextProvider>(typeof (SecurityContextManager));
				return _providers;
			}
		}
		#endregion Properties

		/// <summary>
		/// Gets or sets the current user.
		/// </summary>
		public static UserPrincipal CurrentUser
		{
			get { return Provider.CurrentUser; }
			set { Provider.CurrentUser = value; }
		}
		/// <summary>
		/// Determines if the current user is being impersonated.
		/// </summary>
		public static bool IsImpersonating
		{
			get { return Provider.IsImpersonating; }
		}
		/// <summary>
		/// Gets the current identity (or the impersonated identity)
		/// </summary>
		public static UserIdentity CurrentIdentity
		{
			get { return Provider.CurrentIdentity; }
		}
		/// <summary>
		/// Gets the impersonator or logged in user if no impersonation is taking place.
		/// </summary>
		public static UserIdentity CurrentRealIdentity
		{
			get { return Provider.CurrentRealIdentity; }
		}
		/// <summary>
		/// Determines if the current user is anonymous.
		/// </summary>
		public static bool IsAnonymous
		{
			get { return Provider.IsAnonymous; }
		}
		/// <summary>
		/// Gets the user identity of an authenticated user.
		/// </summary>
		public static Guid UserID
		{
			get { return Provider.IsAnonymous ? Guid.Empty : Provider.CurrentUser.Identity.Ticket.UserSession.UserID; }
		}
	}
}