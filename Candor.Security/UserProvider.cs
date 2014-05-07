using System;
using System.Collections.Generic;
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
        /// Creates a new instance, not initialized.
        /// </summary>
        protected UserProvider() { }
        /// <summary>
        /// Creates a new instance, initialized with a specific name, but with default property configuration values.
        /// </summary>
        /// <param name="name"></param>
        protected UserProvider(string name)
        {
            InitializeInternal(name, new NameValueCollection());
        }

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
        public Int32 HashGroupMaximum { get; set; }
        /// <summary>
        /// Gets or sets the amount of time a remembered session (on a private computer) should be in minutes.
        /// </summary>
        public Int32 ExtendedSessionDuration { get; set; }
        /// <summary>
        /// Gets or sets the amount of time a session on a public computer should be in minutes.
        /// </summary>
        public Int32 PublicSessionDuration { get; set; }
        /// <summary>
        /// Gets or sets the number of hours a password reset code is valid for.
        /// Any amount of time past this would just require a password reset to be emailed again.
        /// </summary>
        public Int32 PasswordResetCodeExpirationHours { get; set; }
        /// <summary>
        /// Gets or sets the number of days a newly setup guest user account password is valid for.
        /// Any amount of time past this would just require a password reset to be emailed again.
        /// </summary>
        public Int32 GuestUserExpirationDays { get; set; }
        /// <summary>
        /// Gets the configured hash provider, or the default one if
        /// one was not specifically configured for this authentication provider.
        /// </summary>
        /// <remarks>
        /// This property will throw an exception if the provider that existed
        /// at initialization has since been removed from the <see cref="HashManager"/>.
        /// </remarks>
        [Obsolete(error: false, message: "Do not set a specific HashProvider.  Instead select an available random one for each new hashing need.")]
        protected virtual HashProvider HashProvider
        {
            get
            {
                if (string.IsNullOrEmpty(_hashProviderName))
                    return HashManager.DefaultProvider;
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
            InitializeInternal(name, configValue);
        }
        private void InitializeInternal(string name, NameValueCollection configValue)
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
            HashGroupMaximum = configValue.GetInt32Value("hashGroupMaximum", 1000);

            ExtendedSessionDuration = configValue.GetInt32Value("extendedSessionDuration", 20160); //20160=2weeks
            PublicSessionDuration = configValue.GetInt32Value("publicSessionDuration", 20); //20 minutes default.
            PasswordResetCodeExpirationHours = configValue.GetInt32Value("PasswordResetCodeExpirationHours", 1);
            GuestUserExpirationDays = configValue.GetInt32Value("GuestUserExpirationDays", 14);
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
                                            : new string[] { };
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
        /// <param name="userId">The unique identity.</param>
        /// <returns></returns>
        public abstract User GetUserByID(Guid userId);
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
        public virtual UserIdentity AuthenticateUser(
            string name, string password, UserSessionDurationType duration,
            string ipAddress, ExecutionResults result)
        {
            return AuthenticateUser(name: name, password: password, duration: duration,
                ipAddress: ipAddress, checkHistory: true, allowUpdateHash: true, result: result);
        }
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
        public virtual UserIdentity AuthenticateUser(string token, UserSessionDurationType duration, String ipAddress, ExecutionResults result)
        {
            const string errorMsg = "Authentication token invalid.";
            Guid renewalToken;
            if (!Guid.TryParse(token, out renewalToken))
            {
                result.AppendError(errorMsg);
                return new UserIdentity();
            }
            var session = GetUserSession(renewalToken);
            if (session == null)
            {
                result.AppendError(errorMsg);
                return new UserIdentity();
            }
            var history = GetSessionAuthenticationHistory(session);
            if (history == null)
            {
                result.AppendError(errorMsg);
                return new UserIdentity();
            }
            if (history.IPAddress != ipAddress)
            {	//coming from a new IPAddress, token was stolen or user is coming from a new dynamic IP address (new internet connection?)
                result.AppendError(errorMsg);
                return new UserIdentity(); //force new login with password (essentially approves this new IP address)
                //WARN: is this a valid check?  Can an imposter just fake the source IP?  Could a legitimate user hop IP Addresses during a single session?
            }

            if ((DateTime.UtcNow - session.RenewedDate).Duration() > TimeSpan.FromMinutes(1))
            {   //reduce the number of writes.  Only need to know within a minute how many active users there are.  There may be many json requests for a single page in a minute.
                session.RenewedDate = DateTime.UtcNow;
                session.ExpirationDate = DateTime.UtcNow.AddMinutes(duration == UserSessionDurationType.PublicComputer ? PublicSessionDuration : ExtendedSessionDuration);
                SaveUserSession(session);
            }
            history.UserSession = session;
            return new UserIdentity(history, Name);
        }

        /// <summary>
        /// Base logic to register a full user, or a guest user.  Creates the appropriate records and the proper validation.
        /// </summary>
        /// <param name="user">A user with a raw password which is turned into a password hash as part of registration.</param>
        /// <param name="result">A ExecutionResults instance to add applicable
        /// warning and error messages to.</param>
        /// <returns>A boolean indicating success (true) or failure (false).</returns>
        protected virtual bool RegisterBase(User user, ExecutionResults result)
        {
            var password = user.PasswordHash;
            if (!ValidateName(user.Name, result) || !ValidatePassword(password, result))
                return false;

            var existing = GetUserByName(user.Name);
            if (existing != null)
            {   //seed user table with deleted users with names you don't want users to have
                result.AppendError("The name you specified cannot be used.");
                return false;
            }
            if (user.UserID.Equals(Guid.Empty))
                user.UserID = Guid.NewGuid();

            var hasher = HashManager.SelectProvider();
            var salt = new UserSalt
            {
                PasswordSalt = hasher.GetSalt(),
                UserID = user.UserID,
                HashGroup = new Random(DateTime.Now.Second).Next(HashGroupMinimum, HashGroupMaximum),
                HashName = hasher.Name
            };
            user.PasswordHash = hasher.Hash(salt.PasswordSalt, password,
                                                   salt.HashGroup + BaseHashIterations);
            using (var scope = new System.Transactions.TransactionScope())
            {
                //starts as a lightweight transaction
                SaveUser(user);
                //enlists in a full distributed transaction if users and salts have different connection strings
                SaveUserSalt(salt);
                scope.Complete();
            }
            return true;
        }

        /// <summary>
        /// Registers a new user.  The PasswordHash property should be the actual password.
        /// </summary>
        /// <param name="user">A user with a raw password which is turned into a password hash as part of registration.</param>
        /// <param name="duration">The amount of time that the initial session will be valid.</param>
        /// <param name="ipAddress">The internet address where the user is connecting from.</param>
        /// <param name="result">A ExecutionResults instance to add applicable
        /// warning and error messages to.</param>
        /// <returns>A boolean indicating success (true) or failure (false).</returns>
        public virtual UserIdentity RegisterUser(User user, UserSessionDurationType duration, String ipAddress, ExecutionResults result)
        {
            var password = user.PasswordHash; //grab before it gets hashed.
            if (!RegisterBase(user, result))
                return new UserIdentity();

            return AuthenticateUser(name: user.Name, password: password, duration: duration,
                                    ipAddress: ipAddress, checkHistory: false, allowUpdateHash: false, result: result);
        }

        /// <summary>
        /// Registers a new guest user.  The user is being created by another user
        /// that is inviting this user to join.
        /// </summary>
        /// <param name="user">A user with a raw password which is turned into a password hash as part of registration.</param>
        /// <param name="result">A ExecutionResults instance to add applicable
        /// warning and error messages to.</param>
        /// <returns>The guest password good for 14 days, or another configurable number of days.  
        /// After that initial period the user can request a password reset when joining.</returns>
        public virtual String RegisterGuestUser(User user, ExecutionResults result)
        {
            user.IsGuest = true;
            return !RegisterBase(user, result) ? null 
                : GenerateUserResetCode(user.Name, TimeSpan.FromDays(GuestUserExpirationDays));
        }
        /// <summary>
        /// Authenticates a user with the requested rule options.  This internal method is called
        /// by the other public versions of the method.  Override in a derived class if you want
        /// to change the rule interpretations or add new rules.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="duration"></param>
        /// <param name="ipAddress"></param>
        /// <param name="checkHistory"></param>
        /// <param name="allowUpdateHash"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual UserIdentity AuthenticateUser(string name, string password,
            UserSessionDurationType duration, string ipAddress, bool checkHistory,
            bool allowUpdateHash, ExecutionResults result)
        {
            if (checkHistory)
            {
                var recentFailures = GetRecentFailedUserNameAuthenticationCount(name);
                if (recentFailures > AllowedFailuresPerPeriod)
                    return FailAuthenticateUser(name, ipAddress, result);
            }
            var user = GetUserByName(name);
            if (user == null)
                return FailAuthenticateUser(name, ipAddress, result);
            var salt = GetUserSalt(user.UserID);
            if (salt == null)
                return FailAuthenticateUser(name, ipAddress, result);

            //this should get a named hashProvider used to originally hash the password... 
            //  fallback to 'default' provider in legacy case when we didn't store the name.
            var hasher = !string.IsNullOrEmpty(salt.HashName) ? HashManager.Providers[salt.HashName] : HashManager.DefaultProvider;
            var passwordHash = hasher.Hash(salt.PasswordSalt, password, salt.HashGroup + BaseHashIterations);
            if (user.PasswordHash != passwordHash)
                return FailAuthenticateUser(name, ipAddress, result);
            var session = new UserSession
            {
                CreatedDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddMinutes(duration == UserSessionDurationType.PublicComputer ? PublicSessionDuration : ExtendedSessionDuration),
                UserID = user.UserID,
                RenewalToken = Guid.NewGuid()
            };
            var history = new AuthenticationHistory
            {
                CreatedDate = session.CreatedDate,
                IPAddress = ipAddress,
                IsAuthenticated = true,
                UserName = name,
                SessionID = session.SessionID,
                UserSession = session
            };
            using (var scope = new System.Transactions.TransactionScope())
            {
                if (allowUpdateHash && (hasher.IsObsolete || user.PasswordHashUpdatedDate < DateTime.UtcNow.AddMonths(-1)))
                {
                    //update hashes on regular basis, keeps the iterations in latest range for current users, and with a 'current' hash provider.
                    hasher = HashManager.SelectProvider();
                    salt.PasswordSalt = hasher.GetSalt();
                    salt.HashGroup = new Random(DateTime.Now.Second).Next(HashGroupMinimum, HashGroupMaximum);
                    salt.HashName = hasher.Name;
                    user.PasswordHash = hasher.Hash(salt.PasswordSalt, password, salt.HashGroup + BaseHashIterations);
                    user.PasswordHashUpdatedDate = DateTime.UtcNow;
                    //starts as a lightweight transaction
                    SaveUser(user);
                    //enlists in a full distributed transaction if users and salts have different connection strings
                    SaveUserSalt(salt);
                }
                //either continues distributed transaction if applicable, 
                //  or creates a new lightweight transaction for these two commands
                SaveUserSession(session);
                InsertUserHistory(history);
                scope.Complete();
            }
            return new UserIdentity(history, Name);
        }
        /// <summary>
        /// updates a user's name and/or password.
        /// </summary>
        /// <param name="item">The user details to be saved.  If Password is empty is it not changed.  If specified it should be the new raw password (not a hash).</param>
        /// <param name="currentPassword">The current raw password for the user used to authenticate that the change can be made, or the current resetcode last sent to this user.</param>
        /// <param name="ipAddress">The internet address where the user is connecting from.</param>
        /// <param name="result">A ExecutionResults instance to add applicable
        /// warning and error messages to.</param>
        /// <returns>A boolean indicating success (true) or failure (false).</returns>
        public virtual bool UpdateUser(User item, String currentPassword, String ipAddress, ExecutionResults result)
        {
            if (item.UserID.Equals(Guid.Empty))
                throw new ArgumentException("The user identity must be specified.");
            var user = GetUserByID(item.UserID);
            var salt = GetUserSalt(item.UserID);
            if (user == null || salt == null)
            {
                result.AppendError("The specified user identity does not exist.");
                return false;
            }
            if (salt.ResetCode == currentPassword)
            {
                if (salt.ResetCodeExpiration < DateTime.UtcNow)
                {
                    result.AppendError(
                        user.IsGuest
                        ? "Your account verification code has expired.  Request a password reset to complete your registration."
                        : "Your password reset code has expired.  Request a new one to be sent to you, and then use it immediately.");
                    return false;
                }
                salt.ResetCode = null;
                salt.ResetCodeExpiration = DateTime.UtcNow;
            }
            else
            {
                var rememberMe = !SecurityContextManager.IsAnonymous &&
                                  SecurityContextManager.CurrentUser.Identity.Ticket.UserSession.ExpirationDate >
                                  DateTime.UtcNow.AddMinutes(PublicSessionDuration);
                if (!AuthenticateUser(name: item.Name, password: currentPassword, ipAddress: ipAddress,
                                      duration: rememberMe ? UserSessionDurationType.Extended : UserSessionDurationType.PublicComputer,
                                      allowUpdateHash: false, checkHistory: false, result: result).IsAuthenticated)
                {
                    result.AppendError("Cannot change password due to authentication error with current password.");
                    return false;
                }
            }
            if (user.Name != item.Name)
            {   //user is changing their sign in name.  Make sure the new name is available.
                var nameExisting = GetUserByName(item.Name);
                if (nameExisting != null)
                {
                    result.AppendError("The name you specified cannot be used.");
                    return false;
                }
                user.Name = item.Name;
            }
            if (!String.IsNullOrEmpty(item.PasswordHash))
            {
                var password = item.PasswordHash;
                //update hashes on regular basis, keeps the iterations in latest range for current users, and with a 'current' hash provider.
                HashProvider hasher = HashManager.SelectProvider();
                salt.PasswordSalt = hasher.GetSalt();
                salt.HashGroup = new Random(DateTime.Now.Second).Next(HashGroupMinimum, HashGroupMaximum);
                salt.HashName = hasher.Name;
                user.PasswordHash = hasher.Hash(salt.PasswordSalt, password, salt.HashGroup + BaseHashIterations);
                user.PasswordUpdatedDate = DateTime.UtcNow;
            }
            using (var scope = new System.Transactions.TransactionScope())
            {
                //starts as a lightweight transaction
                SaveUser(user);
                //enlists in a full distributed transaction if users and salts have different connection strings
                SaveUserSalt(salt);
                scope.Complete();
            }
            return true;
        }
        /// <summary>
        /// Generates a new password reset code for a user and stores that as the current code valid
        /// for the next hour.
        /// </summary>
        /// <param name="name">The user name / email address.</param>
        /// <returns>If the user exists, then a reset code string; otherwise null.</returns>
        public virtual String GenerateUserResetCode(String name)
        {
            return GenerateUserResetCode(name, TimeSpan.FromHours(PasswordResetCodeExpirationHours));
        }
        /// <summary>
        /// Generates a new password reset code for a user and stores that as the current code valid
        /// for a configurable time.  Shared logic between normal user and guest user functionality.
        /// </summary>
        /// <param name="name">The user name / email address.</param>
        /// <param name="resetExpiration">The configurable duration the reset code should last.</param>
        /// <returns></returns>
        protected virtual String GenerateUserResetCode(String name, TimeSpan resetExpiration)
        {
            var user = GetUserByName(name);
            if (user == null)
                return null;

            var salt = GetUserSalt(user.UserID);
            if (!String.IsNullOrWhiteSpace(salt.ResetCode) && salt.ResetCodeExpiration > DateTime.UtcNow.AddMinutes(5))
                return salt.ResetCode; //if submits form to request a code multiple times during window, then use the same code unless about to expire.

            HashProvider hasher = !string.IsNullOrEmpty(salt.HashName) ? HashManager.Providers[salt.HashName] : HashManager.DefaultProvider;
            salt.ResetCode = hasher.GetSalt(16);
            salt.ResetCodeExpiration = DateTime.UtcNow.Add(resetExpiration);
            SaveUserSalt(salt);

            return salt.ResetCode;
        }
        /// <summary>
        /// Gets the latest session(s) for a given user.
        /// </summary>
        /// <param name="userId">The unique identity.</param>
        /// <param name="take">The maximum number of sessions to retrieve.</param>
        /// <returns>A list of sessions; If empty then the user has never logged in (such as a no-show guest).</returns>
        public abstract List<UserSession> GetLatestUserSessions(Guid userId, Int32 take);
        /// <summary>
        /// Inserts a new user authentication history.
        /// </summary>
        /// <param name="history"></param>
        protected abstract void InsertUserHistory(AuthenticationHistory history);
        /// <summary>
        /// Saves a user session, insert or update.
        /// </summary>
        /// <param name="session"></param>
        protected abstract void SaveUserSession(UserSession session);
        /// <summary>
        /// Saves a user, insert or update.
        /// </summary>
        /// <param name="user"></param>
        protected abstract void SaveUser(User user);
        /// <summary>
        /// Saves a user salt, insert or update.
        /// </summary>
        /// <param name="salt"></param>
        protected abstract void SaveUserSalt(UserSalt salt);
        /// <summary>
        /// Gets a user's salt metadata.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        protected abstract UserSalt GetUserSalt(Guid userId);
        /// <summary>
        /// Gets a user session by the renewal token.
        /// </summary>
        /// <param name="renewalToken"></param>
        /// <returns></returns>
        protected abstract UserSession GetUserSession(Guid renewalToken);
        /// <summary>
        /// Gets the number of times a user name has failed authentication within the configured allowable failure period.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected abstract int GetRecentFailedUserNameAuthenticationCount(string name);
        /// <summary>
        /// Gets the authentication history for a specific session.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        protected abstract AuthenticationHistory GetSessionAuthenticationHistory(UserSession session);
        /// <summary>
        /// Returns a failed authentication attempt, an anonymous user identity.
        /// </summary>
        /// <param name="name">The user name.</param>
        /// <param name="ipAddress">The IP address the user was coming from.</param>
        /// <param name="result">A container for error messages.</param>
        /// <returns></returns>
        protected UserIdentity FailAuthenticateUser(string name, string ipAddress, ExecutionResults result)
        {
            result.AppendError(LoginCredentialsFailureMessage);
            InsertUserHistory(new AuthenticationHistory
                {
                    CreatedDate = DateTime.UtcNow,
                    UserName = name,
                    IPAddress = ipAddress,
                    IsAuthenticated = false
                });
            return new UserIdentity();
        }
    }
}