using System;

namespace Candor.Security
{
    /// <summary>
    /// A salt and settings for a specific user used for generating the password hash.
    /// </summary>
	public class UserSalt
	{
        /// <summary>
        /// A unique record Id.
        /// </summary>
		public int RecordID { get; set; }
        /// <summary>
        /// The unique user identity.
        /// </summary>
		public Guid UserID { get; set; }
        /// <summary>
        /// The current salt value.
        /// </summary>
		public string PasswordSalt { get; set; }
        /// <summary>
        /// The expiration date of the current reset code for this user's password as set
        /// by a forgot password request or in the generation of a guest user account
        /// temporary password.
        /// </summary>
		public virtual DateTime ResetCodeExpiration { get; set; }
        /// <summary>
        /// A randomly generated long value that can be used as a short term temporary
        /// password to login to the user's account.  Alternatively it may be a
        /// temporary password for a guest user account.
        /// </summary>
		public virtual string ResetCode { get; set; }
        /// <summary>
        /// The number of times this user's password is crytographically hashed as a
        /// modifier to a base number of iterations defined by the hash provider
        /// defined by <see cref="HashName"/>.
        /// </summary>
        public virtual int HashGroup { get; set; }
        /// <summary>
        /// Gets or sets the hash algorithm key name to use when generating the hash.
        /// </summary>
        /// <remarks>
        /// Limit: 10 characters.  This does make the row footprint smaller, but it is not the reason.
        /// The reason to to keep the hash names cryptic to a viewer of the table.  They will need the
        /// configuration file also to know what algorithm that hash key means.
        /// </remarks>
		public virtual String HashName { get; set; }
	}
}