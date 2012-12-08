using System.Configuration.Provider;
using Common.Logging;

namespace Candor.Security
{
	/// <summary>
	/// The base contract that must be fullfilled by any SecurityContext provider.
	/// </summary>
	public abstract class SecurityContextProvider : ProviderBase
	{
		private ILog _logProvider;
		/// <summary>
		/// Gets or sets the log destination for this provider instance.  If not set, it will be automatically loaded when needed.
		/// </summary>
		public ILog LogProvider
		{
			get { return _logProvider ?? (_logProvider = LogManager.GetLogger(typeof (SecurityContextProvider))); }
			set { _logProvider = value; }
		}
		/// <summary>
		/// Gets or sets the current user.
		/// </summary>
		public abstract UserPrincipal CurrentUser { get; set; }
		/// <summary>
		/// Determines if the current user is being impersonated.
		/// </summary>
		public virtual bool IsImpersonating
		{
			get { return CurrentIdentity.Name != CurrentRealIdentity.Name; }
		}
		/// <summary>
		/// Gets the current identity (or the impersonated identity)
		/// </summary>
		public virtual UserIdentity CurrentIdentity
		{
			get { return CurrentUser.Identity; }
		}
		/// <summary>
		/// Gets the impersonator or logged in user if no impersonation is taking place.
		/// </summary>
		public virtual UserIdentity CurrentRealIdentity
		{
			get
			{
				if (CurrentUser.Identity.ImpersonatorIdentity != null)
					return CurrentUser.Identity.ImpersonatorIdentity;
				else
					return CurrentUser.Identity;
			}
		}
		/// <summary>
		/// Determines if the current user is anonymous.
		/// </summary>
		public virtual bool IsAnonymous
		{
			get { return CurrentIdentity == null || CurrentUser.IsAnonymous; }
		}
	}
}