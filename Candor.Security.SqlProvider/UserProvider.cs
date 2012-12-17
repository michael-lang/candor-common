using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using cs = Candor.Security;
using System.Collections.Specialized;
using Candor.Configuration.Provider;
using Candor.Security.Cryptography;
using Candor.Data;

namespace Candor.Security.SqlProvider
{
    /// <summary>
    /// Sql Server implementation of the <see cref="cs.UserProvider"/>.
    /// </summary>
    /// <remarks>
    /// The user and user salt tables can be accessed from a different connection 
    /// than the audit tables if desired for extra security.  You may want to store
    /// the salt for the users in a separate database server to make it less likely
    /// that a hacker would get into both systems and have a means to decrypt the
    /// user password hashes.
    /// If you do not want to do this or are not able to have the extra databases
    /// then just specifify the main 'connectionName' property and it will be used
    /// for all the tables.
    /// Separate connection names / strings is not typical in other providers.  This
    /// is a special case for security reasons.
    /// </remarks>
    public class UserProvider : cs.UserProvider
    {
        private string connectionName_ = string.Empty;
        private string connectionNameUser_ = string.Empty;
        private string connectionNameUserSalt_ = string.Empty;
        private string connectionNameAudit_ = string.Empty;
        /// <summary>
        /// Gets the connection name to the SQL database.
        /// </summary>
        public string ConnectionName
        {
            get { return connectionName_; }
            set { connectionName_ = value; }
        }
        /// <summary>
        /// Gets the connection name to the SQL database for the user table.
        /// </summary>
        public string ConnectionNameUser
        {
            get { return connectionNameUser_; }
            set { connectionNameUser_ = value; }
        }
        /// <summary>
        /// Gets the connection name to the SQL database for the salt table.
        /// </summary>
        public string ConnectionNameUserSalt
        {
            get { return connectionNameUserSalt_; }
            set { connectionNameUserSalt_ = value; }
        }
        /// <summary>
        /// Gets the connection name to the SQL database for the audit tables.
        /// </summary>
        public string ConnectionNameAudit
        {
            get { return connectionNameAudit_; }
            set { connectionNameAudit_ = value; }
        }
        /// <summary>
        /// Gets the connection string to the SQL database.
        /// </summary>
        public string ConnectionString
        {
            get { return GetConnectionString(ConnectionName); }
        }
        /// <summary>
        /// Gets the connection string to the SQL database for the user table.
        /// </summary>
        public string ConnectionStringUser
        {
            get { return GetConnectionString(ConnectionNameUser ?? ConnectionName); }
        }
        /// <summary>
        /// Gets the connection string to the SQL database for the salt table.
        /// </summary>
        public string ConnectionStringUserSalt
        {
            get { return GetConnectionString(ConnectionNameUserSalt ?? ConnectionName); }
        }
        /// <summary>
        /// Gets the connection string to the SQL database for the audit tables.
        /// </summary>
        public string ConnectionStringAudit
        {
            get { return GetConnectionString(ConnectionNameAudit ?? ConnectionName); }
        }

        private string GetConnectionString(String name)
        {
            var cs = System.Configuration.ConfigurationManager.ConnectionStrings[name];
            if (cs == null || String.IsNullOrWhiteSpace(cs.ConnectionString))
                throw new ArgumentException(String.Format("The connection string '{0}' must be specified.", name));
            return cs.ConnectionString;
        }
        /// <summary>
        /// Initializes the provider from the configuration values.
        /// </summary>
        /// <param name="name">The name of this provider instance.</param>
        /// <param name="config">The configuration values.</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            ConnectionName = config.GetStringValue("connectionName", null);
            ConnectionNameUser = config.GetStringValue("connectionNameUser", null);
            ConnectionNameUserSalt = config.GetStringValue("connectionNameUserSalt", null);
            ConnectionNameAudit = config.GetStringValue("connectionNameAudit", null);
        }
        /// <summary>
        /// Gets a user by identity.
        /// </summary>
        /// <param name="name">The unique identity.</param>
        /// <returns></returns>
        public override User GetUserByID(Guid userID)
        {
            using (var cn = new SqlConnection(ConnectionStringUser))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"Select RecordID, UserID, Name, PasswordHash, PasswordHashUpdatedDate, PasswordUpdatedDate, IsDeleted, CreatedDate, CreatedByUserID, UpdatedDate, UpdatedByUserID
 from Security.User
 where UserID = @UserID";
                    cmd.Parameters.AddWithValue("UserID", userID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return BuildUser(reader);
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Gets a user by name.
        /// </summary>
        /// <param name="name">The unique sign in name.</param>
        /// <returns></returns>
        public override User GetUserByName(string name)
        {
            using (var cn = new SqlConnection(ConnectionStringUser))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"Select RecordID, UserID, Name, PasswordHash, PasswordHashUpdatedDate, PasswordUpdatedDate, IsDeleted, CreatedDate, CreatedByUserID, UpdatedDate, UpdatedByUserID
 from Security.User
 where Name = @Name";
                    cmd.Parameters.AddWithValue("Name", name);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return BuildUser(reader);
                        }
                    }
                }
            }
            return null;
        }

        private static User BuildUser(SqlDataReader reader)
        {
            return new User()
            {
                RecordID = reader.GetInt32("RecordID", 0),
                UserID = reader.GetGuid("UserID"),
                Name = reader.GetString("Name", ""),
                PasswordHash = reader.GetString("PasswordHash", null),
                PasswordHashUpdatedDate = reader.GetUTCDateTime("PasswordHashUpdatedDate", DateTime.MinValue),
                PasswordUpdatedDate = reader.GetUTCDateTime("PasswordUpdatedDate", DateTime.MinValue),
                IsDeleted = reader.GetBoolean("IsDeleted", false),
                CreatedDate = reader.GetUTCDateTime("CreatedDate", DateTime.MinValue),
                CreatedByUserID = reader.GetGuid("CreatedByUserID"),
                UpdatedDate = reader.GetUTCDateTime("UpdatedDate", DateTime.MinValue),
                UpdatedByUserID = reader.GetGuid("UpdatedByUserID")
            };
        }
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
        public override UserIdentity AuthenticateUser(
            string name, string password, UserSessionDurationType duration, 
            string ipAddress, ExecutionResults result)
        {
            return AuthenticateUser(name: name, password: password, duration: duration, 
                ipAddress: ipAddress, checkHistory: true, allowUpdateHash: true, result: result);
        }

        private cs.UserIdentity AuthenticateUser(string name, string password, 
            UserSessionDurationType duration, string ipAddress, bool checkHistory, 
            bool allowUpdateHash, ExecutionResults result)
        {
            if (checkHistory)
            {
                List<AuthenticationHistory> recentHistory = GetRecentUserNameAuthenticationHistory(name);
                var recentFailures = 0;
                foreach (var item in recentHistory)
                {
                    if (!item.IsAuthenticated)
                        recentFailures++;
                    else
                        break;
                }
                if (recentFailures > AllowedFailuresPerPeriod)
                    return FailAuthenticateUser(name, ipAddress, result);
            }
            User user = GetUserByName(name);
            if (user == null)
                return FailAuthenticateUser(name, ipAddress, result);
            UserSalt salt = GetUserSalt(user.UserID);
            if (salt == null)
                return FailAuthenticateUser(name, ipAddress, result);
            var passwordHash = HashManager.Hash(salt.PasswordSalt, password, salt.HashGroup + BaseHashIterations);
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
                IPAddress = ipAddress,
                CreatedDate = DateTime.UtcNow,
                IsAuthenticated = true,
                UserName = name,
                UserSession = session
            };
            if (allowUpdateHash && user.PasswordHashUpdatedDate < DateTime.UtcNow.AddMonths(-1))
            {
                //update hashes on regular basis, keeps the iterations in latest range for current users.
                salt.HashGroup = new Random(DateTime.Now.Second).Next(HashGroupMinimum, HasGroupMaximum);
                user.PasswordHash = HashManager.Hash(user.UserSalt.PasswordSalt, password, salt.HashGroup + BaseHashIterations);
                user.PasswordHashUpdatedDate = DateTime.UtcNow;
                SaveUserSalt(salt);
                SaveUser(user);
            }
            InsertUserHistory(history);
            return new cs.UserIdentity(history, this.Name);
        }

        private void InsertUserHistory(AuthenticationHistory history)
        {
            using (var cn = new SqlConnection(ConnectionStringAudit))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"insert into Security.AuthenticationHistory 
 (UserName, IpAddress, IsAuthenticated, CreatedDate)
 Values (@UserName, @IpAddress, @IsAuthenticated, @CreatedDate)";
                    cmd.Parameters.AddWithValue("UserName", history.UserName);
                    cmd.Parameters.AddWithValue("IpAddress", history.IPAddress);
                    cmd.Parameters.AddWithValue("IsAuthenticated", history.IsAuthenticated);
                    cmd.Parameters.AddWithValue("CreatedDate", history.CreatedDate);
                    cmd.ExecuteNonQuery();
                }
            }
            SaveUserSession(history.UserSession);
        }
        private void SaveUserSession(UserSession session)
        {
            using (var cn = new SqlConnection(ConnectionStringAudit))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    if (session.SessionID == 0)
                    {
                        cmd.CommandText = @"insert into Security.UserSession 
 (UserID, RenewalToken, ExpirationDate)
 Values (@UserID, @RenewalToken, @ExpirationDate)";
                        cmd.Parameters.AddWithValue("UserID", session.UserID);
                        cmd.Parameters.AddWithValue("RenewalToken", session.RenewalToken);
                        cmd.Parameters.AddWithValue("ExpirationDate", session.ExpirationDate);
                    }
                    else
                    {
                        cmd.CommandText = @"update Security.UserSession 
 set ExpirationDate = @ExpirationDate,
 RenewedDate = @RenewedDate
 where SessionID = @SessionID";
                        cmd.Parameters.AddWithValue("ExpirationDate", session.ExpirationDate);
                        cmd.Parameters.AddWithValue("RenewedDate", session.RenewedDate);
                        cmd.Parameters.AddWithValue("SessionID", session.SessionID);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void SaveUser(User user)
        {
            using (var cn = new SqlConnection(ConnectionStringUser))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    if (user.RecordID == 0)
                    {
                        cmd.CommandText = @"insert into Security.User 
 (UserID, Name, PasswordHash, PasswordHashUpdatedDate)
 Values (@UserID, @Name, @PasswordHash, getutcdate())";
                        cmd.Parameters.AddWithValue("UserID", user.UserID);
                        cmd.Parameters.AddWithValue("Name", user.Name);
                        cmd.Parameters.AddWithValue("PasswordHash", user.PasswordHash);
                    }
                    else
                    {
                        cmd.CommandText = @"update Security.User 
 set Name = @Name,
 PasswordHash = @PasswordHash,
 PasswordHashUpdatedDate = @PasswordHashUpdatedDate,
 PasswordUpdatedDate = @PasswordUpdatedDate
 IsDeleted = @IsDeleted,
 UpdatedDate = @UpdatedDate,
 UpdatedByUserID = @UpdatedByUserID
 where UserID = @UserID";
                        cmd.Parameters.AddWithValue("Name", user.Name);
                        cmd.Parameters.AddWithValue("PasswordHash", user.PasswordHash);
                        cmd.Parameters.AddWithValue("PasswordHashUpdatedDate", user.PasswordHashUpdatedDate);
                        cmd.Parameters.AddWithValue("PasswordUpdatedDate", user.PasswordUpdatedDate);
                        cmd.Parameters.AddWithValue("IsDeleted", user.IsDeleted);
                        cmd.Parameters.AddWithValue("UpdatedDate", user.UpdatedDate);
                        cmd.Parameters.AddWithValue("UpdatedByUserID", user.UpdatedByUserID);
                        cmd.Parameters.AddWithValue("UserID", user.UserID);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void SaveUserSalt(UserSalt salt)
        {
            using (var cn = new SqlConnection(ConnectionStringUserSalt))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    if (salt.RecordID == 0)
                    {
                        cmd.CommandText = @"insert into Security.UserSalt 
 (UserID, PasswordSalt, HashGroup)
 Values (@UserID, @PasswordSalt, @HashGroup)";
                        cmd.Parameters.AddWithValue("UserID", salt.UserID);
                        cmd.Parameters.AddWithValue("PasswordSalt", salt.PasswordSalt);
                        cmd.Parameters.AddWithValue("HashGroup", salt.HashGroup);
                    }
                    else
                    {
                        cmd.CommandText = @"update Security.UserSalt 
 set PasswordSalt = @PasswordSalt,
 ResetCode = @ResetCode,
 ResetCodeExpiration = @ResetCodeExpiration,
 HashGroup = @HashGroup
 where UserID = @UserID";
                        cmd.Parameters.AddWithValue("PasswordSalt", salt.PasswordSalt);
                        cmd.Parameters.AddWithValue("ResetCode", salt.ResetCode);
                        cmd.Parameters.AddWithValue("ResetCodeExpiration", salt.ResetCodeExpiration);
                        cmd.Parameters.AddWithValue("HashGroup", salt.HashGroup);
                        cmd.Parameters.AddWithValue("UserID", salt.UserID);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private UserSalt GetUserSalt(Guid userID)
        {
            using (var cn = new SqlConnection(ConnectionStringUserSalt))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"Select RecordID, UserID, PasswordSalt, ResetCode, ResetCodeExpiration, HashGroup
 from Security.UserSalt
 where UserID = @UserID";
                    cmd.Parameters.AddWithValue("UserID", userID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserSalt()
                            {
                                RecordID = reader.GetInt32("RecordID", 0),
                                UserID = reader.GetGuid("UserID"),
                                PasswordSalt = reader.GetString("PasswordSalt", null),
                                ResetCode = reader.GetString("ResetCode", null),
                                ResetCodeExpiration = reader.GetUTCDateTime("ResetCodeExpiration", DateTime.MinValue),
                                HashGroup = reader.GetInt32("HashGroup", 0)
                            };
                        }
                    }
                }
            }
            return null;
        }
        private UserSession GetUserSession(Guid renewalToken)
        {
            using (var cn = new SqlConnection(ConnectionStringAudit))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"Select SessionID, UserID, CreatedDate, ExpirationDate, RenewedDate
 From Security.UserSession
 where RenewalToken = @RenewalToken";
                    cmd.Parameters.AddWithValue("RenewalToken", renewalToken);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserSession()
                            {
                                SessionID = reader.GetInt32("SessionID", 0),
                                UserID = reader.GetGuid("UserID"),
                                RenewalToken = renewalToken,
                                CreatedDate = reader.GetUTCDateTime("CreatedDate", DateTime.MinValue),
                                ExpirationDate = reader.GetUTCDateTime("ExpirationDate", DateTime.MinValue),
                                RenewedDate = reader.GetUTCDateTime("RenewedDate", DateTime.MinValue)
                            };
                        }
                    }
                }
            }
            return null;
        }
        private List<AuthenticationHistory> GetRecentUserNameAuthenticationHistory(string name)
        {
            List<AuthenticationHistory> items = new List<AuthenticationHistory>();
            using (SqlConnection cn = new SqlConnection(ConnectionStringAudit))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"Select RecordID, UserName, IpAddress, CreatedDate, IsAuthenticated, SessionID 
 from Security.AuthenticationHistory 
 where UserName = @UserName
 and CreatedDate > @StartDate
 order by CreatedDate desc";
                    cmd.Parameters.AddWithValue("UserName", name);
                    cmd.Parameters.AddWithValue("StartDate", DateTime.UtcNow.AddMinutes(FailurePeriodMinutes));
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new AuthenticationHistory()
                            {
                                RecordID = reader.GetInt64("RecordID", 0),
                                UserName = reader.GetString("UserName", null),
                                IPAddress = reader.GetString("IpAddress", null),
                                CreatedDate = reader.GetUTCDateTime("CreatedDate", DateTime.MinValue),
                                IsAuthenticated = reader.GetBoolean("IsAuthenticated", false),
                                SessionID = reader.GetInt64("SessionID", 0)
                            });
                        }
                    }
                }
            }
            return items;
        }
        private AuthenticationHistory GetSessionAuthenticationHistory(UserSession session)
        {
            using (var cn = new SqlConnection(ConnectionStringAudit))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"Select top 1 RecordID, UserName, IPAddress, CreatedDate, IsAuthenticated, SessionID
 from Security.AuthenticationHistory
 where SessionID = @SessionID
 and IsAuthenticated == 1
 order by CreatedDate desc";
                    cmd.Parameters.AddWithValue("SessionID", session.SessionID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new AuthenticationHistory()
                            {
                                RecordID = reader.GetInt32("RecordID", 0),
                                UserName = reader.GetString("UserName", ""),
                                IPAddress = reader.GetString("IPAddress", ""),
                                CreatedDate = reader.GetUTCDateTime("CreatedDate", DateTime.MinValue),
                                IsAuthenticated = reader.GetBoolean("IsAuthenticated", false),
                                SessionID = session.SessionID,
                                UserSession = session
                            };
                        }
                    }
                }
            }
            return null;
        }

        private cs.UserIdentity FailAuthenticateUser(string name, string ipAddress, ExecutionResults result)
        {
            result.AppendError(LoginCredentialsFailureMessage);
            using (var cn = new SqlConnection(ConnectionStringAudit))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"insert into Security.AuthenticationHistory 
 (UserName, IpAddress, IsAuthenticated)
 Values (@UserName, @IpAddress, @IsAuthenticated)";
                    cmd.Parameters.AddWithValue("UserName", name);
                    cmd.Parameters.AddWithValue("IpAddress", ipAddress);
                    cmd.Parameters.AddWithValue("IsAuthenticated", false);
                    cmd.ExecuteNonQuery();
                }
            }
            return new cs.UserIdentity();
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
        public override UserIdentity AuthenticateUser(string token, UserSessionDurationType duration, String ipAddress, ExecutionResults result)
        {
            String errorMsg = "Authentication token invalid.";
            Guid renewalToken;
            if (!Guid.TryParse(token, out renewalToken))
            {
                result.AppendError(errorMsg);
                return new cs.UserIdentity();
            }
            UserSession session = GetUserSession(renewalToken);
            if (session == null)
            {
                result.AppendError(errorMsg);
                return new cs.UserIdentity();
            }
            AuthenticationHistory history = GetSessionAuthenticationHistory(session);
            if (history == null)
            {
                result.AppendError(errorMsg);
                return new cs.UserIdentity();
            }
            else if (history.IPAddress != ipAddress)
            {	//coming from a new IPAddress, token was stolen or user is coming from a new dynamic IP address (new internet connection?)
                result.AppendError(errorMsg);
                return new cs.UserIdentity(); //force new login with password (essentially approves this new IP address)
                //WARN: is this a valid check?  Can an imposter just fake the source IP?  Could a legitimate user hop IP Addresses during a single session?
            }

            session.RenewedDate = DateTime.UtcNow;
            session.ExpirationDate = DateTime.UtcNow.AddMinutes(duration == UserSessionDurationType.PublicComputer ? PublicSessionDuration : ExtendedSessionDuration);
            SaveUserSession(session);
            history.UserSession = session;
            return new UserIdentity(history, this.Name);
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
        public override UserIdentity RegisterUser(User user, UserSessionDurationType duration, String ipAddress, ExecutionResults result)
        {
            if (!ValidateName(user.Name, result) || !ValidatePassword(user.PasswordHash, result))
                return new cs.UserIdentity();
            if (!ValidateName(user.Name, result))
                return new cs.UserIdentity();

            string password = user.PasswordHash;
            if (!ValidatePassword(password, result))
                return new cs.UserIdentity();

            var existing = GetUserByName(user.Name);
            if (existing != null)
            {
                result.AppendError("The name you specified cannot be used.");
                return new cs.UserIdentity();
            }
            if (user.UserID.Equals(Guid.Empty))
                user.UserID = Guid.NewGuid();

            var salt = new UserSalt
            {
                PasswordSalt = HashManager.GetSalt(),
                UserID = user.UserID,
                HashGroup = new Random(DateTime.Now.Second).Next(HashGroupMinimum, HasGroupMaximum)
            };
            user.PasswordHash = HashManager.Hash(salt.PasswordSalt, password,
                                                   salt.HashGroup + BaseHashIterations);
            SaveUser(user);
            SaveUserSalt(salt);
            return AuthenticateUser(name: user.Name, password: password, duration: duration,
                                    ipAddress: ipAddress, checkHistory: false, allowUpdateHash: false, result: result);
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
        public override bool UpdateUser(User item, String currentPassword, String ipAddress, ExecutionResults result)
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
                        "Your password reset code has expired.  Request a new one to be sent to you, and then use it immediately.");
                    return false;
                }
                salt.ResetCode = null;
                salt.ResetCodeExpiration = DateTime.UtcNow;
            }
            else
            {
                var rememberMe = !cs.SecurityContextManager.IsAnonymous &&
                                  cs.SecurityContextManager.CurrentUser.Identity.Ticket.UserSession.ExpirationDate >
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
                salt.HashGroup = new Random(DateTime.Now.Second).Next(HashGroupMinimum, HasGroupMaximum);
                user.PasswordHash = HashManager.Hash(salt.PasswordSalt, password, salt.HashGroup + BaseHashIterations);
                user.PasswordUpdatedDate = DateTime.UtcNow;
            }
            using (var scope = new System.Transactions.TransactionScope())
            {
                //starts as a lightweight transaction
                SaveUser(user);
                //enlists in a full distributed transaction if users and salts have different connection strings
                SaveUserSalt(salt);
            }
            return true;
        }
    }
}