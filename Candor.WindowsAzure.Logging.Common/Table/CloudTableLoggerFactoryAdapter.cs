using System.Collections.Specialized;
using Common.Logging;
using Common.Logging.Factory;

namespace Candor.WindowsAzure.Logging.Common.Table
{
    public class CloudTableLoggerFactoryAdapter : AbstractCachingLoggerFactoryAdapter
    {
        private readonly string _connectionName;

        public CloudTableLoggerFactoryAdapter(NameValueCollection properties)
        {
            _connectionName = properties["ConnectionName"];
        }
        protected override ILog CreateLogger(string name)
        {
            return new CloudTableLogger(_connectionName, name);
        }
    }
}
