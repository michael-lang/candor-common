using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Candor.Configuration.Provider;
using Candor.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table;

namespace Candor.Security.AzureStorageProvider
{
    public class UserProvider : Security.UserProvider
    {
        private string _connectionName = string.Empty;
        private string _connectionNameUser = string.Empty;
        private string _connectionNameUserSalt = string.Empty;
        private string _connectionNameAudit = string.Empty;
        private CloudTableProxy<User> _tableProxyUserById;
        private CloudTableProxy<User> _tableProxyUserByName;
        private CloudTableProxy<UserSalt> _tableProxyUserSalt;
        private CloudTableProxy<AuthenticationHistory> _tableProxyAuthenticationHistoryByUserName;
        private CloudTableProxy<AuthenticationHistory> _tableProxyAuthenticationHistoryByToken;
        private CloudTableProxy<UserSession> _tableProxyUserSessionByToken;
        private CloudTableProxy<UserSession> _tableProxyUserSessionByUser;

        public UserProvider() { }

        public UserProvider(String name)
            : base(name)
        {
        }
        public UserProvider(String name, String connectionName)
        {
            InitializeInternal(name, new NameValueCollection());
            ConnectionName = connectionName;
        }
        public UserProvider(String name, String connectionName, String connectionNameUser, String connectionNameUserSalt, String connectionNameAudit)
        {
            InitializeInternal(name, new NameValueCollection());
            ConnectionName = connectionName;
            ConnectionNameUser = connectionNameUser;
            ConnectionNameUserSalt = connectionNameUserSalt;
            ConnectionNameAudit = connectionNameAudit;
        }

        /// <summary>
        /// Gets the connection name to the SQL database.
        /// </summary>
        public string ConnectionName
        {
            get { return _connectionName; }
            set
            {
                _connectionName = value;
                _tableProxyUserById = null;
                _tableProxyUserByName = null;
                _tableProxyUserSalt = null;
                _tableProxyUserSessionByToken = null;
                _tableProxyUserSessionByUser = null;
                _tableProxyAuthenticationHistoryByToken = null;
                _tableProxyAuthenticationHistoryByUserName = null;
            }
        }
        /// <summary>
        /// Gets the connection name to the SQL database for the user table.
        /// </summary>
        public string ConnectionNameUser
        {
            get { return _connectionNameUser; }
            set
            {
                _connectionNameUser = value;
                _tableProxyUserById = null;
                _tableProxyUserByName = null;
            }
        }
        /// <summary>
        /// Gets the connection name to the SQL database for the salt table.
        /// </summary>
        public string ConnectionNameUserSalt
        {
            get { return _connectionNameUserSalt; }
            set
            {
                _connectionNameUserSalt = value;
                _tableProxyUserSalt = null;
            }
        }
        /// <summary>
        /// Gets the connection name to the SQL database for the audit tables.
        /// </summary>
        public string ConnectionNameAudit
        {
            get { return _connectionNameAudit; }
            set
            {
                _connectionNameAudit = value;
                _tableProxyUserSessionByToken = null;
                _tableProxyUserSessionByUser = null;
                _tableProxyAuthenticationHistoryByToken = null;
                _tableProxyAuthenticationHistoryByUserName = null;
            }
        }
        private CloudTableProxy<User> TableProxyUserById
        {
            get
            {
                if (_tableProxyUserById == null)
                {
                    _tableProxyUserById = new CloudTableProxy<User>
                    {
                        ConnectionName = ConnectionNameUser ?? ConnectionName,
                        TableName = String.Format("{0}ById", typeof(User).Name)
                    };
                    _tableProxyUserById.SetPartitionRowKeyAsGuid(x => x.UserID);
                }
                return _tableProxyUserById;
            }
        }
        private CloudTableProxy<User> TableProxyUserByName
        {
            get
            {
                return _tableProxyUserByName ?? (_tableProxyUserByName = new CloudTableProxy<User>
                    {
                        ConnectionName = ConnectionNameUser ?? ConnectionName,
                        TableName = typeof (User).Name,
                        PartitionKey = x => x.Name.GetValidPartitionKey(),
                        RowKey = x => x.UserID.ToString().GetValidRowKey()
                    });
            }
        }
        private CloudTableProxy<UserSalt> TableProxyUserSalt
        {
            get
            {
                if (_tableProxyUserSalt == null)
                {
                    _tableProxyUserSalt = new CloudTableProxy<UserSalt>
                    {
                        ConnectionName = ConnectionNameUserSalt ?? ConnectionName,
                        TableName = typeof(UserSalt).Name
                    };
                    _tableProxyUserSalt.SetPartitionRowKeyAsGuid(x => x.UserID);
                }
                return _tableProxyUserSalt;
            }
        }
        private CloudTableProxy<AuthenticationHistory> TableProxyAuthenticationHistoryByUserName
        {
            get
            {
                return _tableProxyAuthenticationHistoryByUserName ??
                       (_tableProxyAuthenticationHistoryByUserName = new CloudTableProxy<AuthenticationHistory>
                       {   //paritioned by user name, row always has latest entered record as first (lowest) row key
                               ConnectionName = ConnectionNameAudit ?? ConnectionName,
                               PartitionKey = x => string.Format("UserName|{0}", x.UserName).GetValidPartitionKey(),
                               RowKey = x => String.Format("{0:d19}", DateTime.MaxValue.Ticks - x.CreatedDate.Ticks).GetValidRowKey()
                           });
            }
        }
        private CloudTableProxy<AuthenticationHistory> TableProxyAuthenticationHistoryByToken
        {
            get
            {
                if (_tableProxyAuthenticationHistoryByToken == null)
                {
                    _tableProxyAuthenticationHistoryByToken = new CloudTableProxy<AuthenticationHistory>
                    {
                        ConnectionName = ConnectionNameAudit ?? ConnectionName,
                        PartitionKey = x => "ByToken",
                        RowKey = x => String.Format("{0}", x.UserSession.RenewalToken).GetValidRowKey()
                    };
                }
                return _tableProxyAuthenticationHistoryByToken;
            }
        }
        private CloudTableProxy<UserSession> TableProxyUserSessionByToken
        {
            get
            {
                if (_tableProxyUserSessionByToken == null)
                {
                    _tableProxyUserSessionByToken = new CloudTableProxy<UserSession>
                    {
                        ConnectionName = ConnectionNameAudit ?? ConnectionName,
                        PartitionKey = x => "ByToken",
                        RowKey = x => x.RenewalToken.ToString().GetValidRowKey()
                    };
                }
                return _tableProxyUserSessionByToken;
            }
        }
        private CloudTableProxy<UserSession> TableProxyUserSessionByUser
        {
            get
            {
                return _tableProxyUserSessionByUser ?? (_tableProxyUserSessionByUser = new CloudTableProxy<UserSession>
                    {   //paritioned by user Id, row always has latest entered record as first (lowest) row key
                        ConnectionName = ConnectionNameAudit ?? ConnectionName,
                        PartitionKey = x => String.Format("UserId|{0}", x.UserID).GetValidPartitionKey(),
                        RowKey = x => String.Format("{0:d19}", DateTime.MaxValue.Ticks - x.CreatedDate.Ticks).GetValidRowKey()
                    });
            }
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
            var user = TableProxyUserById.Get(userId);
            return user == null ? null : user.Entity;
        }
        /// <summary>
        /// Gets a user by name.
        /// </summary>
        /// <param name="name">The unique sign in name.</param>
        /// <returns></returns>
        public override User GetUserByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "User name must be supplied.");
            var user =
                (TableProxyUserByName.GetPartition(name.GetValidPartitionKey()) ?? new List<TableEntityProxy<User>>())
                    .Find(x => String.Equals(x.Entity.Name, name, StringComparison.InvariantCultureIgnoreCase));
            return user == null ? null : user.Entity;
        }

        protected override void SaveUser(User user)
        {
            TableProxyUserByName.InsertOrUpdate(user);
            TableProxyUserById.InsertOrUpdate(user);
        }

        protected override void SaveUserSalt(UserSalt salt)
        {
            TableProxyUserSalt.InsertOrUpdate(salt);
        }

        protected override UserSalt GetUserSalt(Guid userId)
        {
            var item = TableProxyUserSalt.Get(userId);
            return item == null ? null : item.Entity;
        }

        /// <summary>
        /// Gets the latest session(s) for a given user.
        /// </summary>
        /// <param name="userId">The unique identity.</param>
        /// <param name="take">The maximum number of sessions to retrieve.</param>
        /// <returns></returns>
        public override List<UserSession> GetLatestUserSessions(Guid userId, Int32 take)
        {
            var proxies = TableProxyUserSessionByUser.GetPartition(String.Format("UserId|{0}", userId), take: take);
            return (proxies == null) ? null : proxies.Select(x => x.Entity).ToList();
        }

        protected override void InsertUserHistory(AuthenticationHistory history)
        {
            TableProxyAuthenticationHistoryByUserName.Insert(history);
            if (history.UserSession != null && !history.UserSession.RenewalToken.Equals(Guid.Empty))
                TableProxyAuthenticationHistoryByToken.InsertOrUpdate(history);
        }

        protected override int GetRecentFailedUserNameAuthenticationCount(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "User name must be supplied.");

            var rkCompare = DateTime.MaxValue.Ticks - DateTime.UtcNow.AddMinutes(-1*FailurePeriodMinutes).Ticks;
            var rowKeyFilter = TableQuery.GenerateFilterCondition(TableConstants.RowKey,
                                                                  QueryComparisons.LessThanOrEqual, //smallest is latest
                                                                  rkCompare.ToString("d19"));
            var attempts =
                TableProxyAuthenticationHistoryByUserName.QueryPartition(String.Format("UserName|{0}", name).GetValidPartitionKey(), 
                rowKeyFilter, take: AllowedFailuresPerPeriod * 2);
            if (attempts == null || attempts.Count == 0)
                return 0;
            //already sorted descending
            return attempts.TakeWhile(t => !t.Entity.IsAuthenticated).Count();
        }

        protected override AuthenticationHistory GetSessionAuthenticationHistory(UserSession session)
        {
            var item = TableProxyAuthenticationHistoryByToken.Get("ByToken", session.RenewalToken.ToString().GetValidRowKey());
            return item == null ? null : item.Entity;
        }

        protected override UserSession GetUserSession(Guid renewalToken)
        {
            var item = TableProxyUserSessionByToken.Get("ByToken", renewalToken.ToString());
            return item == null ? null : item.Entity;
        }
        protected override void SaveUserSession(UserSession session)
        {
            TableProxyUserSessionByUser.InsertOrUpdate(session);
            TableProxyUserSessionByToken.InsertOrUpdate(session);
        }
    }
}
