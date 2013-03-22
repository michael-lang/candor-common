using System.Configuration.Provider;
using Common.Logging;
using System.Collections.Specialized;

namespace Candor.Security
{
    /// <summary>
    /// The base contract that must be fullfilled by any SecurityContext provider.
    /// </summary>
    public abstract class SecurityContextProvider : ProviderBase
    {
        public SecurityContextProvider()
        {
        }

        public SecurityContextProvider(string name)
        {
            InitializeInternal(name, new NameValueCollection());
        }

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
        /// Initializes the provider with the specified values.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <param name="configValue">Provider specific attributes.</param>
        public override void Initialize(string name, NameValueCollection configValue)
        {
            InitializeInternal(name, configValue);
        }

        private void InitializeInternal(string name, NameValueCollection configValue)
        {
            base.Initialize(name, configValue);
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