using System;

namespace Candor.WindowsAzure.Logging.Common
{
    internal class LoggerConfiguration
    {
        public String Name { get; set; }
        public Boolean IsTraceEnabled { get; set; }
        public Boolean IsDebugEnabled { get; set; }
        public Boolean IsErrorEnabled { get; set; }
        public Boolean IsFatalEnabled { get; set; }
        public Boolean IsInfoEnabled { get; set; }
        public Boolean IsWarnEnabled { get; set; }
    }
}
