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
	}
}