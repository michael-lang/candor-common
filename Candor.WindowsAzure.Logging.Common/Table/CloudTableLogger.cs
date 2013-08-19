using System;
using System.ComponentModel;
using Candor.WindowsAzure.Storage.Table;
using Common.Logging;
using Common.Logging.Factory;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Candor.WindowsAzure.Logging.Common.Table
{
    public class CloudTableLogger : AbstractLogger
    {
        private readonly String _loggerName;
        private LoggerConfiguration _configuration;
        private DateTime _configurationLoadTime = DateTime.MinValue;
        private String _connectionName;
        private CloudTableProxy<LogEvent> _tableProxy;
        private CloudTableProxy<LoggerConfiguration> _configurationTableProxy;

        public CloudTableLogger(string connectionName, string name)
        {
            ConnectionName = connectionName;
            _loggerName = name;
        }

        /// <summary>
        /// Gets or sets the connection name to the Azure Table storage.
        /// </summary>
        public string ConnectionName
        {
            get { return _connectionName; }
            set
            {
                _connectionName = value;
                _tableProxy = null;
                _configurationTableProxy = null;
            }
        }
        private CloudTableProxy<LogEvent> TableProxy
        {
            get
            {
                return _tableProxy ?? (_tableProxy = new CloudTableProxy<LogEvent>
                {
                    ConnectionName = ConnectionName,
                    PartitionKey = x => x.CreatedDate.ToString("yyyy-MM-dd"),
                    RowKey = x => String.Format("{0:dd HH:mm:ss.fff}-{1}", x.CreatedDate, Guid.NewGuid())
                });
            }
        }
        private CloudTableProxy<LoggerConfiguration> ConfigurationTableProxy
        {
            get
            {
                return _configurationTableProxy ?? (_configurationTableProxy = new CloudTableProxy<LoggerConfiguration>
                {
                    ConnectionName = ConnectionName,
                    PartitionKey = x => x.Name.GetValidPartitionKey(),
                    RowKey = x => x.Name.GetValidRowKey()
                });
            }
        }
        private LoggerConfiguration Configuration
        {
            get
            {
                if (_configuration != null && (DateTime.UtcNow - _configurationLoadTime).TotalMinutes < 5)
                    return _configuration;
                _configurationLoadTime = DateTime.UtcNow;
                var config = ConfigurationTableProxy.Get(_loggerName.GetValidPartitionKey(),
                                                         _loggerName.GetValidRowKey());
                if (config != null)
                    return _configuration = config.Entity;

                _configuration =
                    new LoggerConfiguration
                        {
                            Name = _loggerName,
                            IsDebugEnabled = true,
                            IsErrorEnabled = true,
                            IsFatalEnabled = true,
                            IsInfoEnabled = true,
                            IsTraceEnabled = true,
                            IsWarnEnabled = true
                        };
                ConfigurationTableProxy.InsertOrUpdate(_configuration);
                return _configuration;
            }
        }

        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            String msgText = null;
            if (message == null)
                msgText = "";
            else if (message is string)
                msgText = (String)message;
            else
            {
                var conv = TypeDescriptor.GetConverter(message.GetType());
                if (conv.CanConvertTo(typeof(string)))
                    msgText = (string)conv.ConvertTo(message, typeof(string));
            }
            
            var le = new LogEvent
                {
                    LoggerName = _loggerName,
                    Level = level,
                    Message = msgText,
                    ExceptionStackTrace = exception != null ? exception.ToString() : null, 
                    CreatedDate = DateTime.UtcNow
                };
            try
            {
                le.ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(null as IFormatProvider);
                le.DeploymentId = RoleEnvironment.IsAvailable ? RoleEnvironment.DeploymentId : "N/A";
                le.RoleName = RoleEnvironment.IsAvailable ? RoleEnvironment.CurrentRoleInstance.Role.Name : "N/A";
            }
            catch (Exception exLog) //lack of ability to load these should not prevent logging from occuring.
            {
                if (le.ExceptionStackTrace == null)
                    le.ExceptionStackTrace = "No Exception in event, but the following occured while logging the event.";
                le.ExceptionStackTrace += String.Format("  {0}{1}", Environment.NewLine, exLog);
            }

            TableProxy.Insert(le);
        }

        public override bool IsTraceEnabled
        {
            get { return Configuration.IsTraceEnabled; }
        }

        public override bool IsDebugEnabled
        {
            get { return Configuration.IsDebugEnabled; }
        }

        public override bool IsErrorEnabled
        {
            get { return Configuration.IsErrorEnabled; }
        }

        public override bool IsFatalEnabled
        {
            get { return Configuration.IsFatalEnabled; }
        }

        public override bool IsInfoEnabled
        {
            get { return Configuration.IsInfoEnabled; }
        }

        public override bool IsWarnEnabled
        {
            get { return Configuration.IsWarnEnabled; }
        }
    }
}
