using System;

namespace Candor.Security
{
	public class UserSalt
	{
		public int RecordID { get; set; }
		public Guid UserID { get; set; }
		public string PasswordSalt { get; set; }
		public virtual DateTime ResetCodeExpiration { get; set; }
		public virtual string ResetCode { get; set; }
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