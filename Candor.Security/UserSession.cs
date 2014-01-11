using System;

namespace Candor.Security
{
    /// <summary>
    /// A single authenticated session for a user.
    /// </summary>
	public class UserSession
	{
        /// <summary>
        /// The unique generated auto-increment identifier of the session,
        /// generally not shared externally.
        /// </summary>
		public long SessionID { get; set; }
        /// <summary>
        /// The unique user identity that is authenticated.
        /// </summary>
		public Guid UserID { get; set; }
        /// <summary>
        /// The unique random identity of the session that can be used to
        /// authenticate the user as long as it is still valid.  This should
        /// not be shared outside the user secured headers or cookies before
        /// it expires.
        /// </summary>
		public Guid RenewalToken { get; set; }
        /// <summary>
        /// The date and time when the session began.
        /// </summary>
		public DateTime CreatedDate { get; set; }
        /// <summary>
        /// The date and time when the session will expired if it is not
        /// renewed beforehand.
        /// </summary>
		public DateTime ExpirationDate { get; set; }
        /// <summary>
        /// The date and time when the session was last renewed by the user.
        /// Typically the last time they hit a page (but it isn't renewed
        /// each page hit if renewed less than a minute ago).
        /// </summary>
		public DateTime RenewedDate { get; set; }
	}
}