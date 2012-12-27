using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Candor.Configuration.Provider;

namespace Candor.Tasks.MultiWorkerService
{
    partial class CandorWorkerRoleService : ServiceBase
    {
        private WorkerRole workerRole_;
        public CandorWorkerRoleService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var tasks = new ProviderCollection<WorkerRoleTask>(typeof(WorkerRole));
            workerRole_ = new WorkerRole(tasks);
            workerRole_.OnStart();
        }
#if DEBUG
        protected virtual void OnStop(string[] args)
        {
            OnStop();
        }
#endif
        protected override void OnStop()
        {
            if (workerRole_ != null)
                workerRole_.OnStop();
        }
    }
}
