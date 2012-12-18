using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web.Security;

namespace Candor.Security
{
    /// <summary>
    /// Contains a user's system identity plus a means to get all
    /// capabilities for the user.
    /// </summary>
    [Serializable]
    public class UserPrincipal : MarshalByRefObject, IPrincipal
    {
        private readonly IPrincipal _sourcePrincipal = null;
        private List<string> _roles = null;

        /// <summary>
        /// Creates an anonymous principal
        /// </summary>
        public UserPrincipal()
        {
            Identity = new UserIdentity();
        }
        /// <summary>
        /// Creates a principal from another principal
        /// </summary>
        public UserPrincipal(UserPrincipal principal, String authType)
        {
            _sourcePrincipal = principal;
            Identity = principal == null || principal.Identity == null ? new UserIdentity() : new UserIdentity(principal.Identity.Ticket, authType);
        }
        /// <summary>
        /// Creates a principal from another identity
        /// </summary>
        public UserPrincipal(UserIdentity identity)
        {
            Identity = identity;
        }

        /// <summary>
        /// Gets the user identity details for this principal.
        /// </summary>
        /// <remarks>
        /// This override exists solely to meet the interface requirement.
        /// The other <see cref="Identity"/> property is of the wrong
        /// return type.
        /// </remarks>
        IIdentity IPrincipal.Identity
        {
            get
            {
                if (_sourcePrincipal != null)
                    return _sourcePrincipal.Identity;
                return Identity;
            }
        }
        /// <summary>
        /// Gets the user identity details for this principal.
        /// </summary>
        public UserIdentity Identity { get; private set; }
        /// <summary>
        /// Determines if this user is anonymous.
        /// </summary>
        public bool IsAnonymous
        {
            get
            {
                if (!Identity.IsAuthenticated)
                    return true;
                if (string.IsNullOrWhiteSpace(Identity.Name))
                    return true;
                if (Identity.Name.Equals("anonymous", StringComparison.InvariantCultureIgnoreCase))
                    return true;
                return false;
            }
        }
        /// <summary>
        /// Gets the user identity of an authenticated user.
        /// </summary>
        public Guid UserID
        {
            get { return IsAnonymous ? Guid.Empty : Identity.Ticket.UserSession.UserID; }
        }
        /// <summary>
        /// Gets the user identity of an authenticated user.
        /// </summary>
        public String Name
        {
            get { return IsAnonymous ? "Anonymous" : Identity.Name; }
        }

        /// <summary>
        /// Gets a list of the role names this user is within.
        /// </summary>
        /// <returns></returns>
        public List<string> GetReadOnlyRoleNames()
        {
            EnsureRolesLoadedIfAuthenticated();
            var roles = new List<string>();
            if (_roles != null)
                roles.AddRange(_roles);
            return roles;
        }
        /// <summary>
        /// Determines if this user has access to perform actions of the specified role.
        /// </summary>
        /// <param name="role">The role name.  Use a value from KnownRoles for 
        /// standard role names.</param>
        /// <returns></returns>
        public bool IsInRole(string role)
        {
            EnsureRolesLoadedIfAuthenticated();
            return _roles != null && _roles.Exists(r => r == role);
        }
        /// <summary>
        /// Determines if this user has access to perform actions in any of the specified roles.
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        public bool IsInAnyRole(params string[] roles)
        {
            EnsureRolesLoadedIfAuthenticated();
            var roleNames = new List<string>(roles);
            return _roles != null && _roles.Exists(roleNames.Contains);
        }
        private void EnsureRolesLoadedIfAuthenticated()
        {
            if (_roles == null && Identity != null && Identity.IsAuthenticated)
            {
                _roles = new List<string>(Roles.GetRolesForUser(Identity.Name));
                _roles.Sort();
            }
        }
    }
}