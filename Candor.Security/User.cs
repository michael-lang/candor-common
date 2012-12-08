using System;

namespace Candor.Security
{
	public class User
	{
		public int RecordID { get; set; }
		public Guid UserID { get; set; }
		public string Name { get; set; }
		public string PasswordHash { get; set; }
		public DateTime? PasswordHashUpdatedDate { get; set; }
		public DateTime PasswordUpdatedDate { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime CreatedDate { get; set; }
		public Guid CreatedByUserID { get; set; }
		public DateTime UpdatedDate { get; set; }
		public Guid UpdatedByUserID { get; set; }
		public UserSalt UserSalt { get; set; }
	}
}