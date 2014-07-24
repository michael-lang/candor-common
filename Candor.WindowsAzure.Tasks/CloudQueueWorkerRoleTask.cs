using System;
using Candor.Tasks;
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
        private CloudTableProxy<JobStatus> _messageStatusTableProxy;
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
                _messageStatusTableProxy = null;
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
        /// Gets or sets the amount of time to wait to reprocess a message that failed.
        /// </summary>
        /// <remarks>
        /// To disable the failure wait to reprocess logic, then leave this value at
        /// the default of 0.0.
        /// </remarks>
        public double MessageFailedSkipSeconds { get; set; }
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
        /// A table proxy for job status by MessageId.
        /// </summary>
        public CloudTableProxy<JobStatus> MessageStatusTableProxy
        {
            get
            {
                return _messageStatusTableProxy ?? (_messageStatusTableProxy = new CloudTableProxy<JobStatus>
                {
                    ConnectionName = ConnectionName,
                    PartitionKey = x => x.QueueName.GetValidPartitionKey(),
                    RowKey = x => string.Format("M|{0}|S|{1}", x.MessageId, x.Success).GetValidRowKey()
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
            MessageFailedSkipSeconds = configValue.GetDoubleValue("MessageExceptionSkipSeconds", 0.0);
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
        /// Obsolete.  Use OnWaitingPeriodElapsedAdvanced instead.
        /// </summary>
        [Obsolete]
        public override void OnWaitingPeriodElapsed()
        {   //never called with advanced implementation overridden
        }

        /// <summary>
        /// Continues processing after the waiting period has elapsed.
        /// </summary>
        public override IterationResult OnWaitingPeriodElapsedAdvanced()
        {
            var queue = QueueProxy.GetQueue();
            while (IsRunning)
            {
                CloudQueueMessage message = null;
                try
                {
                    message = queue.GetMessage();
                    if (message == null)
                        return new IterationResult();

                    var statEntity = MessageStatusTableProxy.Get(queue.Name.GetValidPartitionKey(),
                        string.Format("M|{0}|S|{1}", message.Id, false));
                    var status = statEntity != null && statEntity.Entity != null
                        ? statEntity.Entity
                        : CreateJobStatus(message);

                    if (MessageFailedSkipSeconds > double.Epsilon
                        && !status.Success
                        && status.RunDateTime > DateTime.UtcNow.AddSeconds(-1*MessageFailedSkipSeconds))
                        break; //skip this message temporarily and continue with the next.

                    //this lets us know the processing has started.
                    StatusLatestTableProxy.InsertOrUpdate(status);

                    var result = ProcessMessageAdvanced(message, status);
                    status.Success = result.Success;
                    status.RunDateTime = DateTime.UtcNow;
                    if (status.Success)
                        queue.DeleteMessage(message);
                    else //log status record for failed processing
                    {
                        //record every failure by date/time
                        StatusTableProxy.InsertOrUpdate(status);
                        //in another table, record latest failure only for each message.
                        MessageStatusTableProxy.InsertOrUpdate(status);
                    }

                    //always update latest status for this queue
                    StatusLatestTableProxy.InsertOrUpdate(status);
                    
                    if (result.DelayNextMessageSeconds > Double.Epsilon)
                        return new IterationResult { NextWaitingPeriodSeconds = result.DelayNextMessageSeconds };
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
                    if (message == null) continue;
                    var errorStatus = CreateJobStatus(message);
                    errorStatus.Success = false;
                    errorStatus.LogMessage = ex.ToString();
                    MessageStatusTableProxy.InsertOrUpdate(errorStatus);
                }
            }
            return new IterationResult();
        }

        private JobStatus CreateJobStatus(CloudQueueMessage message)
        {
            return new JobStatus
            {
                QueueName = QueueProxy.GetQueue().Name,
                MessageId = message.Id,
                Success = true,
                RunDateTime = DateTime.UtcNow,
                Message = message.AsString
            };
        }
        /// <summary>
        /// Processes the message from the queue, to be implemented by a derived class.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool ProcessMessage(CloudQueueMessage message);
        /// <summary>
        /// Processes the message from the queue and updates the status of the job run.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual bool ProcessMessage(CloudQueueMessage message, JobStatus status)
        {
            status.Success = ProcessMessage(message);
            return status.Success;
        }
        /// <summary>
        /// Processes the message from the queue and updates the status of the job run
        /// and returns some advanced result options.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual ProcessMessageResult ProcessMessageAdvanced(CloudQueueMessage message, JobStatus status)
        {
            var success = ProcessMessage(message, status);
            return new ProcessMessageResult
            {
                DelayNextMessageSeconds = 0.0,
                Success = success
            };
        }
    }
}