using System;
using Candor.Tasks;
using Candor.WindowsAzure.Storage;
using Candor.Configuration.Provider;
using Candor.WindowsAzure.Storage.Queue;
using Candor.WindowsAzure.Storage.Table;
using Common.Logging;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Candor.WindowsAzure.Tasks
{
    public abstract class CloudQueueWorkerRoleTask<T> : RepeatingWorkerRoleTask
        where T : class, new()
    {
        private string _connectionName;
        private String _queueName;
        private CloudQueueProxy<T> _queueProxy;
        private CloudTableProxy<JobStatus> _statusTableProxy;
        private CloudTableProxy<JobStatus> _statusLatestTableProxy;
        private ILog _logProvider;

        /// <summary>
        /// Gets or sets the connection name to the Azure Queue storage.
        /// </summary>
        public string ConnectionName
        {
            get { return _connectionName; }
            set
            {
                _connectionName = value;
                _queueProxy = null;
                _statusTableProxy = null;
                _statusLatestTableProxy = null;
            }
        }
        /// <summary>
        /// Gets or sets the queue name of the Azure Queue storage.
        /// </summary>
        public string QueueName
        {
            get { return _queueName; }
            set
            {
                _queueName = value;
                _queueProxy = null;
            }
        }
        public CloudQueueProxy<T> QueueProxy
        {
            get
            {
                return _queueProxy ??
                       (_queueProxy =
                        new CloudQueueProxy<T>
                        {
                            ConnectionName = ConnectionName,
                            QueueName =  QueueName
                        });
            }
        }
        public CloudTableProxy<JobStatus> StatusTableProxy
        {
            get
            {
                return _statusTableProxy ?? (_statusTableProxy = new CloudTableProxy<JobStatus>
                {
                    ConnectionName = ConnectionName,
                    PartitionKey = x => x.QueueName.GetValidPartitionKey(),
                    RowKey = x => x.RunDateTime.ToString("yyyyMMdd-HHmmss-tttt").GetValidRowKey()
                });
            }
        }
        public CloudTableProxy<JobStatus> StatusLatestTableProxy
        {
            get
            {
                return _statusLatestTableProxy ?? (_statusLatestTableProxy = new CloudTableProxy<JobStatus>
                {
                    ConnectionName = ConnectionName,
                    PartitionKey = x => x.QueueName.GetValidPartitionKey(),
                    RowKey = x => x.Success.ToString()
                });
            }
        }
        protected new ILog LogProvider
        {
            get { return _logProvider ?? (_logProvider = LogManager.GetLogger(GetType())); }
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection configValue)
        {
            ConnectionName = configValue.GetStringValue("connectionName", null);
            base.Initialize(name, configValue);
        }

        public override void OnStart()
        {
            QueueProxy.GetQueue().CreateIfNotExists();
            base.OnStart(); //start timers
        }

        public override void OnWaitingPeriodElapsed()
        {
            var queue = QueueProxy.GetQueue();
            while (IsRunning)
            {
                try
                {
                    var message = queue.GetMessage();
                    if (message == null)
                        return;


                    var status = new JobStatus
                        {
                            QueueName = queue.Name,
                            Success = true,
                            RunDateTime = DateTime.UtcNow
                        };
                    StatusLatestTableProxy.InsertOrUpdate(status);

                    if (ProcessMessage(message))
                        queue.DeleteMessage(message);

                    status.RunDateTime = DateTime.UtcNow;
                    StatusLatestTableProxy.InsertOrUpdate(status);
                    StatusTableProxy.InsertOrUpdate(status);
                }
                catch (OperationCanceledException e)
                {
                    if (!IsRunning) continue;
                    LogProvider.Info(e.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    LogProvider.Error("Unhandled exception.", ex);
                }
            }
        }

        public abstract bool ProcessMessage(CloudQueueMessage message);
    }
}