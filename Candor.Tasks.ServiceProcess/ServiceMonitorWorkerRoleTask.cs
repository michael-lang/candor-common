using Common.Logging;
using System;
using System.Collections.Specialized;
using System.ServiceProcess;
using System.Management;
using Candor.Configuration.Provider;
using System.IO;

namespace Candor.Tasks.ServiceProcess
{
    /// <summary>
    /// Monitors that a service is up either directly or by checking for updates to a
    /// file that it generates.  When a monitored service stops working, attempts are
    /// made to restart the service or the server it runs on.
    /// </summary>
    /// <remarks>
    /// The service being monitored can be on any machine in the same domain.  If not
    /// on a domain, then only the current machine can be monitored and restarted.
    /// </remarks>
    public class ServiceMonitorWorkerRoleTask : RepeatingWorkerRoleTask
    {
        private static ILog _logProvider;
        /// <summary>
        /// Gets or sets the log destination for this type.  If not set, it will be automatically loaded when needed.
        /// </summary>
        public new static ILog LogProvider
        {
            get { return _logProvider ?? (_logProvider = LogManager.GetLogger(typeof(ServiceMonitorWorkerRoleTask))); }
            set { _logProvider = value; }
        }
        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        public String ServiceName { get; set; }
        /// <summary>
        /// Gets or sets the machine name hosting the service.
        /// </summary>
        public String ServiceMachineName { get; set; }
        /// <summary>
        /// Gets or sets the full path of the file to be watched.
        /// </summary>
        public String OutputFileNameToWatch { get; set; }
        /// <summary>
        /// Gets or sets the expected age in minutes of the watched file if the service is
        /// running properly.
        /// </summary>
        public Int32 OutputFileExpectedAgeMinutes { get; set; }
        /// <summary>
        /// Gets or sets the acceptable age in minutes of the watched file before the file
        /// should be reported as an error.
        /// </summary>
        public Int32 OutputFileMaxAgeMinutes { get; set; }

        private const int REBOOT = 2;
        private const int FORCE = 4;
        private const int SHUTDOWN = 8;

        /// <summary>
        /// Initializes the provider with the specified values.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <param name="configValue">Provider specific attributes.</param>
        public override void Initialize(string name, NameValueCollection configValue)
        {
            base.Initialize(name, configValue);
            ServiceName = configValue.GetStringValue("serviceName", "");
            ServiceMachineName = configValue.GetStringValue("serviceMachineName", "");
            OutputFileNameToWatch = configValue.GetStringValue("outputFileNameToWatch", "");
            OutputFileExpectedAgeMinutes = configValue.GetInt32Value("outputFileExpectedAgeMinutes", 0);
            OutputFileMaxAgeMinutes = configValue.GetInt32Value("outputFileMaxAgeMinutes", 60);
            AssertConfigurationValid();
        }
        /// <summary>
        /// Ensures the configuration is valid, otherwise it throws an ArgumentException.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        protected virtual void AssertConfigurationValid()
        {
            if (String.IsNullOrWhiteSpace(OutputFileNameToWatch) && String.IsNullOrWhiteSpace(ServiceName))
                throw new ArgumentException("No Service or File was specified to be watched.");
        }

        /// <summary>
        /// The code to be executed everytime the waiting period elapses.
        /// </summary>
        /// <remarks>
        /// This will complete before the waiting period until the next iteration begins.
        /// </remarks>
        public override void OnWaitingPeriodElapsed()
        {
            AssertConfigurationValid();
            if (!IsMonitorEnabled())
            {
                LogProvider.DebugFormat("This service monitor is disabled at this time: {0}", Name);
                return;
            }
            bool ok = ValidateServiceFile() && ValidateWindowsService();

            if (!ok)
                ok = RestartService();
            if (!ok)
                ok = RestartServer();
            if (!ok)
            {
                LogProvider.WarnFormat("Non-functional service could not be restarted, '{0}'.", Name);
            }
            else
            {
                LogProvider.DebugFormat("Functional service '{0}'.", Name);
            }
        }
        /// <summary>
        /// Possible expansion point to disable the service monitor at preconfigured times.
        /// </summary>
        /// <returns>True if the service should be monitoring currently; otherwise false.</returns>
        protected virtual bool IsMonitorEnabled()
        {
            return true;
        }

        /// <summary>
        /// Validates that a service file exists, and if not it is logged.
        /// A custom logger should be provided to notify the appropriate party.
        /// </summary>
        /// <returns></returns>
        protected bool ValidateServiceFile()
        {
            if (string.IsNullOrEmpty(OutputFileNameToWatch)) { return true; }

            FileInfo fileInfo;
            try
            {
                fileInfo = new FileInfo(OutputFileNameToWatch);
            }
            catch (Exception acquireEx)
            {
                LogProvider.WarnFormat("Could not find file '{0}'.", acquireEx, OutputFileNameToWatch);
                return false;
            }
            DateTime lastWrite;
            try
            {
                lastWrite = fileInfo.LastWriteTime;
            }
            catch (IOException accessEx)
            {
                LogProvider.WarnFormat("Could not determine last access time for '{0}'.", accessEx, OutputFileNameToWatch);
                return false;
            }
            if (lastWrite.AddMinutes(OutputFileMaxAgeMinutes) < DateTime.Now)
            {
                if (lastWrite.AddMinutes(OutputFileMaxAgeMinutes * 10) < DateTime.Now)
                {   //this file access time is likely old because it is inaccessible,
                    //  or this service has been ignored WAY to long.
                    LogProvider.WarnFormat("'{0}' service cannot accurately check for updated file '{1}'.  The file is inaccessible or severely out of date!",
                        Name, OutputFileNameToWatch);
                }
                else
                {
                    LogProvider.WarnFormat("'{0}' service has not updated file '{1}' in {2} minutes.  The normal maximum age is {3} minutes old.",
                        Name, OutputFileNameToWatch,
                        DateTime.Now.Subtract(lastWrite).TotalMinutes,
                        GetExpectedAgeMinutes());
                }
                return false;
            }
            LogProvider.DebugFormat("'{0}' service has updated file '{1}' {2} minutes ago.",
                                    Name, OutputFileNameToWatch,
                                    DateTime.Now.Subtract(lastWrite).TotalMinutes);
            return true;
        }
        /// <summary>
        /// Gets the expected file age in minutes, or the max file age if the
        /// expected age is not specified.
        /// </summary>
        /// <returns></returns>
        protected Int32 GetExpectedAgeMinutes()
        {
            if (OutputFileExpectedAgeMinutes > 0)
                return OutputFileExpectedAgeMinutes;
            return OutputFileMaxAgeMinutes;
        }

        /// <summary>
        /// Validates if a windows service is running.
        /// </summary>
        /// <returns></returns>
        protected bool ValidateWindowsService()
        {
            try
            {
                if (string.IsNullOrEmpty(ServiceName)) { return true; }

                ServiceController serviceController;
                if (!string.IsNullOrEmpty(ServiceMachineName))
                    serviceController = new ServiceController(
                    ServiceName, ServiceMachineName);
                else
                    serviceController = new ServiceController(ServiceName);

                if (serviceController.Status != ServiceControllerStatus.Running)
                {
                    LogProvider.WarnFormat("Service '{0}' is not running.", Name);
                    return false;
                }
                LogProvider.DebugFormat("Service '{0}' is running.", Name);
                return true;
            }
            catch (Exception ex)
            {
                LogProvider.FatalFormat("Failed to control service '{0}'.", ex, Name);
                return false;
            }
        }
        /// <summary>
        /// Restarts the windows service, if possible.
        /// </summary>
        /// <returns></returns>
        protected bool RestartService()
        {
            try
            {
                if (string.IsNullOrEmpty(ServiceName)) { return false; }

                LogProvider.WarnFormat("Attempting to restart service '{0}'.", Name);

                ServiceController serviceController;
                if (!string.IsNullOrEmpty(ServiceMachineName))
                    serviceController = new ServiceController(
                    ServiceName, ServiceMachineName);
                else
                    serviceController = new ServiceController(ServiceName);

                if (serviceController.Status != ServiceControllerStatus.Stopped)
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                }

                if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    serviceController.Start();
                }

                while (serviceController.Status != ServiceControllerStatus.Running)
                {
                    serviceController.WaitForStatus(ServiceControllerStatus.Running);
                }
                LogProvider.WarnFormat("Restarted service '{0}'.", Name);
                return true;
            }
            catch (Exception ex)
            {
                LogProvider.FatalFormat("Failed to restart service '{0}'.", ex, Name);
                return false;
            }
        }
        /// <summary>
        /// Force Restarts the service, if possible.
        /// </summary>
        /// <returns></returns>
        protected bool RestartServer()
        {
            try
            {
                if (ServiceMachineName.Length == 0) { return false; }

                LogProvider.WarnFormat("Attempting to restart server monitored by '{0}'.", Name);

                var server = new ManagementScope
                    {
                        Path = new ManagementPath(FormatServerName(ServiceMachineName)),
                        Options = {Impersonation = ImpersonationLevel.Impersonate, EnablePrivileges = true}
                    };

                var oQuery = new ObjectQuery("select name from Win32_OperatingSystem");
                using (var search = new ManagementObjectSearcher(server, oQuery))
                {
                    using (var items = search.Get())
                    {
                        ManagementBaseObject rebootParams;
                        foreach (ManagementObject item in items)
                        {
                            using (rebootParams = item.GetMethodParameters("Win32Shutdown"))
                            {
                                rebootParams["Flags"] = REBOOT + FORCE;
                                rebootParams["Reserved"] = 0;
                                item.InvokeMethod("Win32Shutdown", rebootParams, null);
                            }
                        }
                    }
                }
                LogProvider.WarnFormat("Restarted server monitored by '{0}'.", Name);
                return true;
            }
            catch (Exception ex)
            {
                LogProvider.FatalFormat("Failed to restart server monitored by '{0}'.", ex, Name);
                return false;
            }
        }
        /// <summary>
        /// Formats a server name for use by management API to restart the server.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected string FormatServerName(string name)
        {
            //->  \\computerName\root\cimv2
            if (!name.Contains("\\"))
            {
                name = "\\\\" + name + "\\root\\cimv2";
            }
            else if (!name.StartsWith("\\"))
            {
                name = "\\\\" + name;
            }
            return name;
        }
    }
}