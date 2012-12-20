using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Text.RegularExpressions;
using Candor.Configuration.Provider;
using Candor.Security.Cryptography;

namespace Candor.Security
{
	/// <summary>
	/// The base contract that must be fullfilled by any User provider.
	/// </summary>
	public abstract class UserProvider : ProviderBase
    {
		private string _hashProviderName = string.Empty;
		private Regex _emailRegex;
		private string _emailRegexExpression;
		private Regex _nameRegex;
		private string _nameRegexExpression;
		private Regex _passwordRegex;
		private string _passwordRegexExpression;
		/// <summary>
		/// A regular expression to validate names.
		/// </summary>
		public virtual string NameRegexExpression
		{
			get { return _nameRegexExpression; }
			set
			{
				_nameRegexExpression = value;
				_nameRegex = null;
			}
		}
		/// <summary>
		/// Gets the name regular expression instance for the configured expression.
		/// </summary>
		public virtual Regex NameRegex
		{
			get { return _nameRegex ?? (_nameRegex = new Regex(NameRegexExpression, RegexOptions.Compiled)); }
		}
		/// <summary>
		/// A regular expression to validate emails.
		/// </summary>
		public virtual string EmailRegexExpression
		{
			get { return _emailRegexExpression; }
			set
			{
				_emailRegexExpression = value;
				_emailRegex = null;
			}
		}
		/// <summary>
		/// Gets the email regular expression instance for the configured expression.
		/// </summary>
		public virtual Regex EmailRegex
		{
			get { return _emailRegex ?? (_emailRegex = new Regex(EmailRegexExpression, RegexOptions.Compiled)); }
		}
		/// <summary>
		/// A regular expression to validate passwords.
		/// </summary>
		public virtual string PasswordRegexExpression
		{
			get { return _passwordRegexExpression; }
			set
			{
				_passwordRegexExpression = value;
				_passwordRegex = null;
			}
		}
		/// <summary>
		/// Gets or sets the validation expression that new passwords must meet.
		/// </summary>
		public virtual Regex PasswordRegex
		{
			get { return _passwordRegex ?? (_passwordRegex = new Regex(PasswordRegexExpression, RegexOptions.Compiled)); }
		}
		/// <summary>
		/// An error message shown when the password does not match the required format.
		/// </summary>
        public virtual string PasswordErrorMessage { get; set; }
        /// <summary>
        /// The minimal number of hash iterations of a password.
        /// </summary>
        /// <remarks>
        /// This should not change after going live in a database, unless 
        /// deployment coordinated with updating hashgroup of all existing 
        /// users at the same time.  However, the preferred option would be
        /// to change the <see cref="HashGroupMinimum"/> and <see cref="HashGroupMaximum"/>
        /// to change future new hashes to be of a higher mimimum security.
        /// </remarks>
        public Int32 BaseHashIterations { get; set; }
        /// <summary>
        /// Gets the minimum hash group to be set when users authenticate by password or register a new account.
        /// </summary>
        /// <remarks>This can change as often as desired without affecting storage of existing user password hashes.</remarks>
        public Int32 HashGroupMinimum { get; set; }
        /// <summary>
        /// Gets the maximum hash group to be set when users authenticate by password or register a new account.
        /// </summary>
        /// <remarks>This can change as often as desired without affecting storage of existing user password hashes.</remarks>
        public Int32 HasGroupMaximum { get; set; }
		/// <summary>
		/// Gets or sets the amount of time a remembered session (on a private computer) should be.
		/// </summary>
		public Int32 ExtendedSessionDuration { get; set; }
		/// <summary>
		/// Gets or sets the amount of time a session on a public computer should be.
		/// </summary>
		public Int32 PublicSessionDuration { get; set; }
		/// <summary>
		/// Gets the configured hash provider, or the default one if
		/// one was not specifically configured for this authentication provider.
		/// </summary>
		/// <remarks>
		/// This property will throw an exception if the provider that existed
        /// at initialization has since been removed from the <see cref="HashManager"/>.
		/// </remarks>
		protected virtual HashProvider HashProvider
		{
			get
			{
				if (string.IsNullOrEmpty(_hashProviderName))
					return HashManager.Provider;
				HashProvider hashProvider = HashManager.Providers[_hashProviderName];
				if (hashProvider == null)
					// ReSharper disable NotResolvedInText
					throw new ArgumentOutOfRangeException("HashProviderName", _hashProviderName,
					                                      "The specified HashProviderName does not exist or has been removed.");
				// ReSharper restore NotResolvedInText
				return hashProvider;
			}
		}
		/// <summary>
		/// Gets or sets the number of days before passwords expire.  Leave 0 for no expiration.
		/// </summary>
		public int PasswordExpiration { get; set; }
		/// <summary>
		/// Gets or sets the message to display when the login fails due to too many failures.
		/// </summary>
		public virtual string LoginExceededFailureMessage { get; set; }
		/// <summary>
		/// Gets or sets the message to display when the login fails due to invalid credentials.
		/// </summary>
		public virtual string LoginCredentialsFailureMessage { get; set; }
		/// <summary>
		/// Gets or sets the number of minutes to check for previous failed logins.
		/// </summary>
		public virtual int FailurePeriodMinutes { get; set; }
		/// <summary>
		/// Gets or sets the number of allowed failed logins per user in a given
		/// period.
		/// </summary>
		public virtual int AllowedFailuresPerPeriod { get; set; }
		/// <summary>
		/// Gets or sets the roles that can impersonate other users
		/// </summary>
		public virtual string[] ImpersonationAllowedRoles { get; set; }
		/// <summary>
		/// Initializes the provider with the specified values.
		/// </summary>
		/// <param name="name">The name of the provider.</param>
		/// <param name="configValue">Provider specific attributes.</param>
		public override void Initialize(string name, NameValueCollection configValue)
		{
			base.Initialize(name, configValue);
			NameRegexExpression = configValue.GetStringValue("nameRegexExpression", "^([a-zA-Z0-9\\-_\\s])*$");
			EmailRegexExpression = configValue.GetStringValue("emailRegexExpression",
			                                                  "^([0-9a-zA-Z]+[-\\._+&]*)*[0-9a-zA-Z]+@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$");
			PasswordRegexExpression = configValue.GetStringValue("passwordRegexExpression", "^([a-zA-Z0-9@*#]{6,128})$");
			PasswordErrorMessage = configValue.GetStringValue("passwordErrorMessage",
                                                              "The password must be between 6 and 32 characters long; and can only contain letters, numbers, and these special symbols(@, *, #)");
            BaseHashIterations = configValue.GetInt32Value("baseHashIterations", 5000);
            HashGroupMinimum = configValue.GetInt32Value("hashGroupMinimum", 1);
            HasGroupMaximum = configValue.GetInt32Value("hasGroupMaximum", 1000);

			ExtendedSessionDuration = configValue.GetInt32Value("extendedSessionDuration", 20160); //20160=2weeks
			PublicSessionDuration = configValue.GetInt32Value("publicSessionDuration", 20); //20 minutes default.
			_hashProviderName = configValue.GetStringValue("hashProviderName", String.Empty);
			if (!string.IsNullOrEmpty(_hashProviderName))
            {
                // ReSharper disable NotResolvedInText
				if (HashManager.Providers[_hashProviderName] == null)
					throw new ArgumentOutOfRangeException("hashProviderName", _hashProviderName,
                                                          "The specified HashProviderName does not exist.");
                // ReSharper restore NotResolvedInText
			}
			PasswordExpiration = configValue.GetInt32Value("passwordExpiration", PasswordExpiration);
			LoginExceededFailureMessage = configValue.GetStringValue("loginExceededFailureMessage",
			                                                         "Too many recent failed attempts have been made for this name.  " +
			                                                         "Either you entered the wrong password or an account by this name does not exist.  " +
			                                                         "If this is your account, try again later or email support with your problem.  " +
			                                                         "For security do not include your password in any email.");
			LoginCredentialsFailureMessage = configValue.GetStringValue("loginCredentialsFailureMessage",
			                                                            "The supplied name or password is incorrect.");
			FailurePeriodMinutes = configValue.GetInt32Value("failurePeriodMinutes", 20);
			AllowedFailuresPerPeriod = configValue.GetInt32Value("allowedFailuresPerPeriod", 5);
			String impersonateRoles = configValue.GetStringValue("impersonationAllowedRoles", "Administrator");
			ImpersonationAllowedRoles = !String.IsNullOrEmpty(impersonateRoles)
				                            ? impersonateRoles.Split(';', ',')
				                            : new string[] {};
		}
		/// <summary>
		/// Validates that a password meets minimum requirements.
		/// </summary>
		/// <param name="password"></param>
		/// <param name="results"></param>
		/// <returns></returns>
		public virtual bool ValidatePassword(string password, ExecutionResults results)
		{
			if (!PasswordRegex.IsMatch(password))
			{
				results.AppendError(PasswordErrorMessage);
				return false;
			}
			return true;
		}
		/// <summary>
		/// Validates that a string is a valid email address format.
		/// </summary>
		/// <returns></returns>
		public virtual bool ValidateEmailAddressFormat(string emailAddress)
		{
			return Regex.IsMatch(emailAddress, EmailRegexExpression);
		}
		/// <summary>
		/// Validates that the specified name meets minimum requirements.
		/// </summary>
		/// <param name="name">The desired name/alias.</param>
		/// <param name="result">Any error messages about the desired name.</param>
		/// <returns></returns>
		public virtual bool ValidateName(string name, ExecutionResults result)
		{
			if (!NameRegex.IsMatch(name) && !EmailRegex.IsMatch(name))
			{
				//if this message is changed, do the same anywhere else it is used
				result.AppendError(
					"The name contains invalid characters.  The name must be an email address OR contain only letters, numbers, dashes, underscores, and spaces, are allowed.");
				return false;
			}
			return true;
		}
        /// <summary>
        /// Gets a user by identity.
        /// </summary>
        /// <param name="name">The unique identity.</param>
        /// <returns></returns>
        public abstract User GetUserByID(Guid userID);
        /// <summary>
        /// Gets a user by name.
        /// </summary>
        /// <param name="name">The unique sign in name.</param>
        /// <returns></returns>
        public abstract User GetUserByName(string name);
		/// <summary>
		/// Authenticates against the data store and returns a UserIdentity given 
		/// a user name, and password.
		/// </summary>
		/// <param name="name">The unique user name.</param>
		/// <param name="password">The matching password.</param>
		/// <param name="ipAddress">The internet address where the user is connecting from.</param>
		/// <param name="duration">The amount of time that the issued token will be valid.</param>
		/// <param name="result">A ExecutionResults instance to add applicable
		/// warning and error messages to.</param>
		/// <returns>
		/// A valid user instance.  If the user did not exist or the 
		/// credentials are incorrect then the IsAuthenticated flag
		/// will be false.  If the credentials were correct the 
		/// IsAuthenticated flag will be true.
		/// </returns>
		public abstract UserIdentity AuthenticateUser( string name, string password, UserSessionDurationType duration, string ipAddress, 
		                                              ExecutionResults result);
		/// <summary>
		/// Authenticates against the data store and returns a UserIdentity given
		/// a token returned from a previous authentication.
		/// </summary>
		/// <param name="token">The unique token.</param>
		/// <param name="duration">The amount of time that the renewed token will be valid.</param>
		/// <param name="ipAddress">The internet address where the user is connecting from.</param>
		/// <param name="result">A ExecutionResults instance to add applicable
		/// warning and error messages to.</param>
		/// <returns>
		/// A valid user identity instance.  If the token is incorrect or expired
		/// then the IsAuthenticated flag will be false.  Otherwise the identity
		/// will be authenticated.
		/// </returns>
		public abstract UserIdentity AuthenticateUser( string token, UserSessionDurationType duration, String ipAddress, ExecutionResults result );
		/// <summary>
		/// Registers a new user.  The PasswordHash property should be the actual password.
		/// </summary>
		/// <param name="user">A user with a raw password which is turned into a password hash as part of registration.</param>
		/// <param name="duration">The amount of time that the initial session will be valid.</param>
		/// <param name="ipAddress">The internet address where the user is connecting from.</param>
		/// <param name="result">A ExecutionResults instance to add applicable
		/// warning and error messages to.</param>
		/// <returns>A boolean indicating success (true) or failure (false).</returns>
		public abstract UserIdentity RegisterUser( User user, UserSessionDurationType duration, String ipAddress, ExecutionResults result );
		/// <summary>
		/// updates a user's name and/or password.
		/// </summary>
		/// <param name="item">The user details to be saved.  If Password is empty is it not changed.  If specified it should be the new raw password (not a hash).</param>
		/// <param name="currentPassword">The current raw password for the user used to authenticate that the change can be made, or the current resetcode last sent to this user.</param>
		/// <param name="ipAddress">The internet address where the user is connecting from.</param>
		/// <param name="result">A ExecutionResults instance to add applicable
		/// warning and error messages to.</param>
		/// <returns>A boolean indicating success (true) or failure (false).</returns>
		public abstract bool UpdateUser( User item, String currentPassword, String ipAddress, ExecutionResults result );
	}
}