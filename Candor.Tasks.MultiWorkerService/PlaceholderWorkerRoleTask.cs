using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candor.Tasks.MultiWorkerService
{
    /// <summary>
    /// This task does nothing except serve as a placeholder sample when
    /// no other tasks need to be run.  In practice, don't configure any
    /// tasks and don't start the service.
    /// </summary>
    public class PlaceholderWorkerRoleTask : WorkerRoleTask
    {
        private ILog LogProvider = LogManager.GetLogger(typeof(PlaceholderWorkerRoleTask));

        public override void OnStart()
        {
            LogProvider.DebugFormat("Started placeholder task '{0}'", this.Name);
        }

        public override void OnStop()
        {
            LogProvider.DebugFormat("Stopped placeholder task '{0}'", this.Name);
        }

        public override void Ping()
        {
            LogProvider.DebugFormat("Pinged placeholder task '{0}'", this.Name);
        }
    }
}
