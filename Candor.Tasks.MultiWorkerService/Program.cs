using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ServiceProcess;

namespace Candor.Tasks.MultiWorkerService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServiceTestForm(new CandorWorkerRoleService()));
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { new CandorWorkerRoleService() };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
