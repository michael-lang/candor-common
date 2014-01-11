using System;

namespace Candor.Security
{
    /// <summary>
    /// A record of an attempt to login by user name and password, successful or not.
    /// </summary>
	public class AuthenticationHistory
	{
        /// <summary>
        /// The unique auto-incremented identity of the session.
        /// </summary>
		public long RecordID { get; set; }
        /// <summary>
        /// The user name that tried to login.
        /// </summary>
		public string UserName { get; set; }
        /// <summary>
        /// The IP of the server the attempt came from.
        /// </summary>
		public string IPAddress { get; set; }
        /// <summary>
        /// The date and time of the attempt.
        /// </summary>
		public DateTime CreatedDate { get; set; }
        /// <summary>
        /// Determines if the attempt was successful or not.
        /// </summary>
		public bool IsAuthenticated { get; set; }
        /// <summary>
        /// If authenticated, this is the identity of the session that was created.
        /// </summary>
		public long SessionID { get; set; }
        /// <summary>
        /// If authenticated, this is the Session details;  Otherwise null.
        /// </summary>
		public UserSession UserSession { get; set; }
	}
}