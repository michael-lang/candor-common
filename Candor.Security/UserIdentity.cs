using System;
using System.Security.Principal;

namespace Candor.Security
{
	/// <summary>
	/// Represents an actor that can utilize the application.
	/// </summary>
	[Serializable]
	public class UserIdentity : MarshalByRefObject, IIdentity
	{
		private readonly String _authenticationType;
		/// <summary>
		/// Creates a new instance of UserIdentity.
		/// </summary>
		public UserIdentity() : this(null, null)
		{
		}
		/// <summary>
		/// Creates a user instance from an authentication attempt.
		/// </summary>
		/// <param name="ticket">The authentication results.</param>
		/// <param name="authType">The provider that authentication this identity.</param>
		public UserIdentity(AuthenticationHistory ticket, String authType)
		{
			if (ticket == null)
			{
				Ticket = new AuthenticationHistory
					         {
						         IsAuthenticated = false,
						         UserName = string.Empty,
						         CreatedDate = DateTime.Now,
						         UserSession = new UserSession {ExpirationDate = DateTime.Now, UserID = Guid.Empty}
					         };
				_authenticationType = authType ?? "None";
			}
			else
			{
				Ticket = ticket;
				_authenticationType = authType;
			}
		}
		/// <summary>
		/// Creates a new instance of an identity, but impersonated by the specified user.
		/// </summary>
		/// <param name="userID">The unique identity of the user to impersonate.</param>
		/// <param name="name">The name of the user to impersonate.</param>
		/// <param name="impersonator">The impersonator's authenticated identity.</param>
		public UserIdentity(Guid userID, string name, UserIdentity impersonator)
		{
			while (impersonator.ImpersonatorIdentity != null) // should be max 1 level deep
				impersonator = impersonator.ImpersonatorIdentity; //don't impersonate multiple levels deep.
			if (impersonator == null || !impersonator.IsAuthenticated || impersonator.Ticket == null ||
			    impersonator.Ticket.UserSession == null || impersonator.Ticket.UserSession.ExpirationDate < DateTime.UtcNow)
				throw new ApplicationException("You cannot impersonate at this time, because your session has ended.");
			Ticket = new AuthenticationHistory
				         {
					         CreatedDate = DateTime.Now,
					         IPAddress = impersonator.Ticket.IPAddress,
					         IsAuthenticated = impersonator.Ticket.IsAuthenticated,
					         UserName = name,
					         SessionID = impersonator.Ticket.SessionID,
					         UserSession =
						         new UserSession
							         {
								         CreatedDate = DateTime.Now,
								         ExpirationDate = impersonator.Ticket.UserSession.ExpirationDate,
								         RenewalToken = impersonator.Ticket.UserSession.RenewalToken,
								         RenewedDate = impersonator.Ticket.UserSession.RenewedDate,
								         SessionID = impersonator.Ticket.SessionID,
								         UserID = userID
							         }
				         };
			ImpersonatorIdentity = impersonator;
		}
		/// <summary>
		/// Gets the identity of the user impersonating the current identity, or null if not applicable.
		/// </summary>
		public UserIdentity ImpersonatorIdentity { get; private set; }
		/// <summary>
		/// Gets a ticket that can be reused to authenticate the user
		/// without their full credentials.
		/// </summary>
		public AuthenticationHistory Ticket { get; private set; }
		/// <summary>
		/// Gets a flag indicating if the user is authenticated against
		/// the authentication type.
		/// </summary>
		public bool IsAuthenticated
		{
			get { return Ticket.IsAuthenticated; }
		}
		/// <summary>
		/// Gets the sign in name of the user.
		/// </summary>
		public string Name
		{
			get { return Ticket.UserName; }
		}
		/// <summary>
		/// Gets the type of authentication for this identity.
		/// </summary>
		public string AuthenticationType
		{
			get { return _authenticationType ?? String.Empty; }
		}
	}
}