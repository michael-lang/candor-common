using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace Candor.Security
{
	/// <summary>
	/// A role provider that gets a list of roles from the AuthorizationManager if configured.
	/// </summary>
	public class CandorAuthorizationRoleProvider : RoleProvider
	{
		NameValueCollection _config = null; //storing for use by AuthorizationProvider web service implementation
        ///// <summary>
        ///// gets or sets if this role provider should load roles using the <see cref="AuthorizationManager"/>.
        ///// </summary>
        ///// <remarks>
        ///// If false, the authorization web service will be called directly and without any caching.
        ///// </remarks>
		//public bool UseAuthorizationManager { get; set; }

		/// <summary>
		/// Initializes this role provider given the specified config.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		/// <remarks>
		/// If this provider is configured with 'useAuthorizationManager="false"' then the configuration
		/// will also be passed down to the default web service implementation of an authorization provider
		/// which will fulfull requests to this role provider.
		/// </remarks>
		public override void Initialize( string name, NameValueCollection config )
		{
			_config = config;
			base.Initialize(name, config);

			//if (config["useAuthorizationManager"] != null)
			//    UseAuthorizationManager = System.Convert.ToBoolean(config["useAuthorizationManager"].ToString());
		}
		/// <summary>
		/// Gets all the roles assigned to the given user name.
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		public override string[] GetRolesForUser( string username )
		{
#warning Load roles from database
			return new string[]{};
		}
		/// <summary>
		/// Determines if a user is in a given role.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="roleName"></param>
		/// <returns></returns>
		public override bool IsUserInRole( string username, string roleName )
		{
			return (GetRolesForUser(username) ?? new string[] { }).Contains(roleName);
		}

		#region Not Implemented
		/// <summary>
		/// Not implemented/supported.
		/// </summary>
		public override void AddUsersToRoles( string[] usernames, string[] roleNames )
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// Not implemented/supported.
		/// </summary>
		public override string ApplicationName
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		/// <summary>
		/// Not implemented/supported.
		/// </summary>
		public override void CreateRole( string roleName )
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// Not implemented/supported.
		/// </summary>
		public override bool DeleteRole( string roleName, bool throwOnPopulatedRole )
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// Not implemented/supported.
		/// </summary>
		public override string[] FindUsersInRole( string roleName, string usernameToMatch )
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// Not implemented/supported.
		/// </summary>
		public override string[] GetAllRoles()
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// Not implemented/supported.
		/// </summary>
		public override string[] GetUsersInRole( string roleName )
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// Not implemented/supported.
		/// </summary>
		public override void RemoveUsersFromRoles( string[] usernames, string[] roleNames )
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// Not implemented/supported.
		/// </summary>
		public override bool RoleExists( string roleName )
		{
			throw new NotSupportedException();
		}
		#endregion
	}
}