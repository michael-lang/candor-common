using Common.Logging;
using System;

namespace Candor.WindowsAzure.Logging.Common
{
    internal class LogEvent
    {
        public String LoggerName { get; set; }
        public LogLevel Level { get; set; }
        public String DeploymentId { get; set; }
        public String RoleName { get; set; }
        public String ThreadId { get; set; }
        public String Message { get; set; }
        public String ExceptionStackTrace { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
