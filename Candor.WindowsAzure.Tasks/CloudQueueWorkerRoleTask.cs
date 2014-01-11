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
    /// <summary>
    /// A worker role task that scans an Azure cloud Queue for messages to process.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
        /// <summary>
        /// A proxy for the queue.
        /// </summary>
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
        /// <summary>
        /// A table proxy for job status.
        /// </summary>
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
        /// <summary>
        /// A table proxy for the latest job status record.
        /// </summary>
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
        /// <summary>
        /// A logging provider.
        /// </summary>
        protected new ILog LogProvider
        {
            get { return _logProvider ?? (_logProvider = LogManager.GetLogger(GetType())); }
        }
        /// <summary>
        /// Initializes the worker task with the specified configuration.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configValue"></param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection configValue)
        {
            ConnectionName = configValue.GetStringValue("connectionName", null);
            base.Initialize(name, configValue);
        }
        /// <summary>
        /// Starts the worker task.
        /// </summary>
        public override void OnStart()
        {
            QueueProxy.GetQueue().CreateIfNotExists();
            base.OnStart(); //start timers
        }
        /// <summary>
        /// Continues processing after the waiting period has elapsed.
        /// </summary>
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
        /// <summary>
        /// Processes the message from the queue, to be implemented by a derived class.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool ProcessMessage(CloudQueueMessage message);
    }
}