namespace Candor.Security
{
    /// <summary>
    /// Types of session length.
    /// </summary>
	public enum UserSessionDurationType
	{
		/// <summary>
		/// The session is valid only for the time the person is browsing; about 20 minutes of inactivity ends the session.
		/// </summary>
		PublicComputer = 0,
		/// <summary>
		/// The session is valid for an extended time between visits; about 2 weeks of inactivity keeps the user logged in.
		/// </summary>
		Extended = 1
	}
}