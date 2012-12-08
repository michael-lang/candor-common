using System;

namespace Candor.Security
{
	public class AuthenticationHistory
	{
		public long RecordID { get; set; }
		public string UserName { get; set; }
		public string IPAddress { get; set; }
		public DateTime CreatedDate { get; set; }
		public bool IsAuthenticated { get; set; }
		public long SessionID { get; set; }
		public UserSession UserSession { get; set; }
	}
}