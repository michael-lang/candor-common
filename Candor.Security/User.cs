using System;

namespace Candor.Security
{
    /// <summary>
    /// A person or system able to interactively login and be assigned roles
    /// that can be performed.
    /// </summary>
	public class User
	{
        /// <summary>
        /// A unqiue record auto-increment identity.
        /// </summary>
		public int RecordID { get; set; }
        /// <summary>
        /// The system random generated unique value for this user.
        /// </summary>
		public Guid UserID { get; set; }
        /// <summary>
        /// The unique user picked name used at login.  This may be an email address.
        /// </summary>
		public string Name { get; set; }
        /// <summary>
        /// The final hashed value of this user's password.  At registration,
        /// this may be the raw password about to be hashed.  This value may also
        /// be empty if loading the user from the database such as for another user to view.
        /// </summary>
		public string PasswordHash { get; set; }
        /// <summary>
        /// The date and time when the password was last hashed.  Old hashes may be
        /// used to indicate that a user should change their password, and at the
        /// password change the system should use the latest hash provider to
        /// improve security.
        /// </summary>
		public DateTime? PasswordHashUpdatedDate { get; set; }
        /// <summary>
        /// The date and time when the user last changed the password from the previous different password.
        /// </summary>
		public DateTime PasswordUpdatedDate { get; set; }
        /// <summary>
        /// Determines if this user is deleted.  If deleted they cannot login anymore, but you can view
        /// the user details that did some audited action.
        /// </summary>
		public bool IsDeleted { get; set; }
        /// <summary>
        /// Determines if this is a guest user or not yet validated, false by default.
        /// </summary>
        public bool IsGuest { get; set; }
        /// <summary>
        /// The date and time when the user was created.
        /// </summary>
		public DateTime CreatedDate { get; set; }
        /// <summary>
        /// The identity of the user that created this user, if any.
        /// </summary>
		public Guid CreatedByUserID { get; set; }
        /// <summary>
        /// The date and time when the user name was changed or it was undeleted (anything but password change).
        /// </summary>
		public DateTime UpdatedDate { get; set; }
        /// <summary>
        /// The identity of the user that last updated this user, if any.
        /// </summary>
		public Guid UpdatedByUserID { get; set; }
	}
}