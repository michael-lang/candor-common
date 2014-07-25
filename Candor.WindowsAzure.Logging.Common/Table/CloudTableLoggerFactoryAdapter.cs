
using Common.Logging;
using Common.Logging.Factory;
using Common.Logging.Configuration;

namespace Candor.WindowsAzure.Logging.Common.Table
{
    public class CloudTableLoggerFactoryAdapter : AbstractCachingLoggerFactoryAdapter
    {
        private readonly string _connectionName;

        public CloudTableLoggerFactoryAdapter()
        {   //I guess this is in case the configuration does not contain any args for the factory adapter?
            //  so we'll use the most reasonable default for an app using Azure cloud tables.
            _connectionName = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";
        }
        
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
