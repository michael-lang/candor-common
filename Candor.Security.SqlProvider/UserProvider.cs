using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using cs = Candor.Security;
using System.Collections.Specialized;
using Candor.Configuration.Provider;
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
        public UserProvider()
        {
            ConnectionNameAudit = null;
            ConnectionNameUserSalt = null;
            ConnectionNameUser = null;
        }

        public UserProvider(string name)
            : base(name)
        {
            ConnectionNameAudit = null;
            ConnectionNameUserSalt = null;
            ConnectionNameUser = null;
        }

        public UserProvider(String name, string connectionName)
            : base(name)
        {
            ConnectionNameAudit = null;
            ConnectionNameUserSalt = null;
            ConnectionNameUser = null;
            ConnectionName = connectionName;
        }

        public UserProvider(String name, string connectionNameUser, string connectionNameUserSalt, string connectionNameAudit)
            : base(name)
        {
            ConnectionName = !String.IsNullOrEmpty(connectionNameUser)
                                 ? connectionNameUser
                                 : !String.IsNullOrEmpty(connectionNameUserSalt)
                                       ? connectionNameUserSalt
                                       : connectionNameAudit;
            ConnectionNameUser = connectionNameUser;
            ConnectionNameUserSalt = connectionNameUserSalt;
            ConnectionNameAudit = connectionNameAudit;
        }

        /// <summary>
        /// Gets the connection name to the SQL database.
        /// </summary>
        public string ConnectionName { get; set; }
        /// <summary>
        /// Gets the connection name to the SQL database for the user table.
        /// </summary>
        public string ConnectionNameUser { get; set; }
        /// <summary>
        /// Gets the connection name to the SQL database for the salt table.
        /// </summary>
        public string ConnectionNameUserSalt { get; set; }
        /// <summary>
        /// Gets the connection name to the SQL database for the audit tables.
        /// </summary>
        public string ConnectionNameAudit { get; set; }
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
            InitializeInternal(name, config);
        }
        private void InitializeInternal(string name, NameValueCollection config)
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
        /// <param name="userId">The unique identity.</param>
        /// <returns></returns>
        public override User GetUserByID(Guid userId)
        {
            using (var cn = new SqlConnection(ConnectionStringUser))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"Select RecordID, UserID, Name, Password`, PasswordHashUpdatedDate, PasswordUpdatedDate, IsDeleted, CreatedDate, CreatedByUserID, UpdatedDate, UpdatedByUserID
 from Security.User
 where UserID = @UserID";
                    cmd.Parameters.AddWithValue("UserID", userId);
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
            return new User
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

        protected override void InsertUserHistory(AuthenticationHistory history)
        {
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
                    cmd.Parameters.AddWithValue("UserName", history.UserName);
                    cmd.Parameters.AddWithValue("IpAddress", history.IPAddress);
                    cmd.Parameters.AddWithValue("IsAuthenticated", history.IsAuthenticated);
                    cmd.ExecuteNonQuery();
                }
            }
            SaveUserSession(history.UserSession);
        }
        protected override void SaveUserSession(UserSession session)
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
        protected override void SaveUser(User user)
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
        protected override void SaveUserSalt(UserSalt salt)
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
 (UserID, PasswordSalt, HashGroup, HashName)
 Values (@UserID, @PasswordSalt, @HashGroup, @HashName)";
                        cmd.Parameters.AddWithValue("UserID", salt.UserID);
                        cmd.Parameters.AddWithValue("PasswordSalt", salt.PasswordSalt);
                        cmd.Parameters.AddWithValue("HashGroup", salt.HashGroup);
                        cmd.Parameters.AddWithValue("HashName", salt.HashName);
                    }
                    else
                    {
                        cmd.CommandText = @"update Security.UserSalt 
 set PasswordSalt = @PasswordSalt,
 ResetCode = @ResetCode,
 ResetCodeExpiration = @ResetCodeExpiration,
 HashGroup = @HashGroup,
 HashName = @HashName
 where UserID = @UserID";
                        cmd.Parameters.AddWithValue("PasswordSalt", salt.PasswordSalt);
                        cmd.Parameters.AddWithValue("ResetCode", salt.ResetCode);
                        cmd.Parameters.AddWithValue("ResetCodeExpiration", salt.ResetCodeExpiration);
                        cmd.Parameters.AddWithValue("HashGroup", salt.HashGroup);
                        cmd.Parameters.AddWithValue("HashName", salt.HashName);
                        cmd.Parameters.AddWithValue("UserID", salt.UserID);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override UserSalt GetUserSalt(Guid userId)
        {
            using (var cn = new SqlConnection(ConnectionStringUserSalt))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"Select RecordID, UserID, PasswordSalt, ResetCode, ResetCodeExpiration, HashGroup, HashName
 from Security.UserSalt
 where UserID = @UserID";
                    cmd.Parameters.AddWithValue("UserID", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserSalt
                                {
                                RecordID = reader.GetInt32("RecordID", 0),
                                UserID = reader.GetGuid("UserID"),
                                PasswordSalt = reader.GetString("PasswordSalt", null),
                                ResetCode = reader.GetString("ResetCode", null),
                                ResetCodeExpiration = reader.GetUTCDateTime("ResetCodeExpiration", DateTime.MinValue),
                                HashGroup = reader.GetInt32("HashGroup", 0),
                                HashName = reader.GetString("HashName", null)
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the latest session(s) for a given user.
        /// </summary>
        /// <param name="userId">The unique identity.</param>
        /// <param name="take">The maximum number of sessions to retrieve.</param>
        /// <returns></returns>
        public override List<UserSession> GetLatestUserSessions(Guid userId, Int32 take)
        {
            var items = new List<UserSession>();
            using (var cn = new SqlConnection(ConnectionStringAudit))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "Select top " + take + @" SessionID, UserID, RenewalToken, CreatedDate, ExpirationDate, RenewedDate
 From Security.UserSession
 where UserID = @UserID
 order by CreatedDate desc";
                    cmd.Parameters.AddWithValue("UserID", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            items.Add(new UserSession
                            {
                                SessionID = reader.GetInt32("SessionID", 0),
                                UserID = reader.GetGuid("UserID"),
                                RenewalToken = reader.GetGuid("RenewalToken"),
                                CreatedDate = reader.GetUTCDateTime("CreatedDate", DateTime.MinValue),
                                ExpirationDate = reader.GetUTCDateTime("ExpirationDate", DateTime.MinValue),
                                RenewedDate = reader.GetUTCDateTime("RenewedDate", DateTime.MinValue)
                            });
                        }
                    }
                }
            }
            return items;
        }
        protected override UserSession GetUserSession(Guid renewalToken)
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
                            return new UserSession
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
        protected override Int32 GetRecentFailedUserNameAuthenticationCount(string name)
        {
            using (var cn = new SqlConnection(ConnectionStringAudit))
            {
                cn.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"Select count(RecordID)
 from Security.AuthenticationHistory 
 where UserName = @UserName
 and CreatedDate > @StartDate
 order by CreatedDate desc";
 #warning This should only check for attempts where IsAuthenticated == 0, and failures after the last success?
                    cmd.Parameters.AddWithValue("UserName", name);
                    cmd.Parameters.AddWithValue("StartDate", DateTime.UtcNow.AddMinutes(-FailurePeriodMinutes));
                    return (Int32)cmd.ExecuteScalar();
                }
            }
        }
        protected override AuthenticationHistory GetSessionAuthenticationHistory(UserSession session)
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
                            return new AuthenticationHistory
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
    }
}