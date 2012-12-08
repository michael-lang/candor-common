using System;

namespace Candor.Security
{
	public class UserSession
	{
		public long SessionID { get; set; }
		public Guid UserID { get; set; }
		public Guid RenewalToken { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime ExpirationDate { get; set; }
		public DateTime RenewedDate { get; set; }
	}
}