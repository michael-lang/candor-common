using System;
using System.Collections.Specialized;
using Candor.Configuration.Provider;
using Candor.WindowsAzure.Storage.Queue;
using Candor.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table;

namespace Candor.Security.AzureStorageProvider
{
    /// <summary>
    /// Sends notifications to users regarding their account.
    /// </summary>
    public class UserNotificationProvider : Security.UserNotificationProvider
    {
        private string _connectionName;
        private CloudQueueProxy<User> _queueProxy;

        /// <summary>
        /// Gets or sets the connection name to the Azure Table storage.
        /// </summary>
        public string ConnectionName
        {
            get { return _connectionName; }
            set
            {
                _connectionName = value;
                _queueProxy = null;
            }
        }
        private CloudQueueProxy<User> QueueProxy
        {
            get
            {
                return _queueProxy ??
                       (_queueProxy =
                        new CloudQueueProxy<User>
                        {
                            ConnectionName = ConnectionName,
                            QueueName = "UserForgotPassword"
                        });
            }
        }
        /// <summary>
        /// Creates a new instance, initialized with a name equaling the type name.
        /// </summary>
        public UserNotificationProvider()
        {
            InitializeInternal(typeof(UserNotificationProvider).Name, new NameValueCollection());
        }
        /// <summary>
        /// Creates a new instance, initialized with a specific name.
        /// </summary>
        /// <param name="name"></param>
        public UserNotificationProvider(string name)
        {
            InitializeInternal(name, new NameValueCollection());
        }
        /// <summary>
        /// Initializes the provider.  This is already called by both constructors.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            InitializeInternal(name, config);
        }
        private void InitializeInternal(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            ConnectionName = config.GetStringValue("connectionName", null);
        }

        /// <summary>
        /// Sends a password reset notification to the specified user.
        /// </summary>
        /// <param name="userName">The user to notify.</param>
        /// <param name="result">
        /// A placeholder for errors in the system, such as email not available now.
        /// Do not notify caller if the user does not exist, for security reasons.</param>
        /// <remarks></remarks>
        public override bool NotifyPasswordReset(String userName, ExecutionResults result)
        {
            var user = ProviderResolver<Security.UserProvider>.Get.Provider.GetUserByName(userName);
            QueueProxy.AddRecordChangeNotification(new RecordChangeNotification
            {
                TableName = typeof(User).Name,
                OperationType = user != null ? TableOperationType.Replace : TableOperationType.Insert,
                PartitionKey = userName.GetValidPartitionKey(),
                RowKey = user != null ? user.UserID.ToString().GetValidRowKey() : ""
            });
            return true;
        }
    }
}
