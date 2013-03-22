using System;
using System.Web;

namespace Candor.Security.Web
{
	/// <summary>
	/// Implements a security context by using the <see cref="System.Web.HttpContext"/> to store the current user.
	/// </summary>
	public class WebSecurityContextProvider : SecurityContextProvider
	{
	    public WebSecurityContextProvider()
	    {
	    }

	    public WebSecurityContextProvider(string name) : base(name)
	    {
	    }

	    /// <summary>
		/// Gets or sets the current user.
		/// </summary>
		public override UserPrincipal CurrentUser
		{
			get
			{
				if (HttpContext.Current == null)
					throw new InvalidOperationException("Cannot get the current HttpContext.  Use a different provider implementation for non web applications.");

				UserPrincipal user = null;
				if (HttpContext.Current.User is UserPrincipal)
					user = HttpContext.Current.User as UserPrincipal;
				else if (HttpContext.Current.User != null && HttpContext.Current.User.Identity is UserIdentity)
				{	//System.Web.Security.RoleProvider - gets swapped out in PostAuthenticateRequest, not sure how to turn off.
					//http://www.asp.net/web-forms/tutorials/security/roles/role-based-authorization-vb
					HttpContext.Current.User = user = new UserPrincipal((UserIdentity)HttpContext.Current.User.Identity);
				}

				if (user == null)
					user = new UserPrincipal(); //anonymous
				return user;
			}
			set
			{
				if (HttpContext.Current == null)
					throw new InvalidOperationException("Cannot get the current HttpContext.  Use a different provider implementation for non web applications.");

				UserPrincipal user = value;
				if (value == null || value.Identity == null)
				{
					user = new UserPrincipal(); // anonymous
				}

				HttpContext.Current.User = user;
			}
		}
	}
}