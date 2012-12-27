using Candor.Configuration.Provider;
using Common.Logging;
using System;
using System.Threading;

namespace Candor.Tasks
{
    /// <summary>
    /// A container for running any number of configured background tasks.
    /// </summary>
    /// <remarks>
    /// This is designed to be run within a windows service or a Windows Azure
    /// worker role instance.
    /// </remarks>
    public class WorkerRole
    {
        private ILog LogProvider = LogManager.GetLogger(typeof(WorkerRole));
        private System.Threading.Timer mainTimer_ = null;
        private object timerLock_ = new object();
        private object iterationLock_ = new object();
        private ProviderCollection<WorkerRoleTask> tasks_ = null;

        public Boolean IsRunning { get; private set; }
        /// <summary>
        /// Gets or sets the interval of time between ping to each task to ensure they are alive.
        /// </summary>
        /// <remarks>
        /// The default is 5 minutes.
        /// </remarks>
        public TimeSpan PingInterval { get; set; }

        private WorkerRole()
        {
            IsRunning = false; //just clarifying the default bool value is intentional
            PingInterval = TimeSpan.FromMinutes(5);
        }
        public WorkerRole(ProviderCollection<WorkerRoleTask> tasks)
            : this()
        {
            tasks_ = tasks;
        }

        public void OnStart()
        {
            try
            {
                if (tasks_ == null || tasks_.Count == 0)
                {
                    LogProvider.Warn("WorkerRole has no tasks configured.  Exiting.");
                    OnStop();
                    return;
                }

                foreach(WorkerRoleTask task in tasks_)
                {
                    LogProvider.InfoFormat("WorkerRole is starting task '{0}'", task.Name);
                    task.OnStart();
                }
                IsRunning = true;

                StartTimer();
                LogProvider.Info("WorkerRole has started.");
            }
            catch (Exception ex)
            {
                LogProvider.Fatal("WorkerRole failed to start.", ex);
                throw;
            }
        }

        void mainTimer__Elapsed(object sender)
        {
            lock (iterationLock_)
            {
                try
                {
                    PauseTimer();
                    LogProvider.Debug("WorkerRole is still alive.");
                    foreach (WorkerRoleTask task in tasks_)
                    {
                        task.Ping();
                    }
                }
                catch (Exception ex)
                {
                    LogProvider.Error("WorkerRole failed to ping.", ex);
                }
                finally
                {
                    try
                    {
                        ResumeTimer();
                    }
                    catch (Exception ex)
                    {
                        LogProvider.Error("WorkerRole could not resume the timer.", ex);
                        if (IsRunning)
                            ResetTimer();
                    }
                }
            }
        }
        
        private void PauseTimer()
        {
            lock (timerLock_)
            {
                mainTimer_.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        private void ResumeTimer()
        {
            lock (timerLock_)
            {
                mainTimer_.Change(PingInterval, PingInterval);
            }
        }
        private void ClearTimer()
        {
            try
            {
                lock (timerLock_)
                {
                    if (mainTimer_ != null)
                    {
                        mainTimer_.Change(Timeout.Infinite, Timeout.Infinite);
                        mainTimer_.Dispose();
                        mainTimer_ = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogProvider.Error("WorkerRole could not clear the timer.", ex);
            }
        }
        private void StartTimer()
        {
            try
            {
                lock (timerLock_)
                {
                    mainTimer_ = new System.Threading.Timer(new TimerCallback(this.mainTimer__Elapsed), null, PingInterval, PingInterval);
                }
            }
            catch (Exception ex)
            {
                LogProvider.Error("WorkerRole could not start the timer.", ex);
            }
        }
        private void ResetTimer()
        {
            try
            {
                ClearTimer();
                StartTimer();
            }
            catch (Exception)
            {	//exceptions were logged already
            }
        }
        public void OnStop()
        {
            IsRunning = false;
            ClearTimer();
            if (tasks_ != null && tasks_.Count > 0)
            {
                foreach (WorkerRoleTask task in tasks_)
                {
                    LogProvider.InfoFormat("WorkerRole is stopping task '{0}'", task.Name);
                    task.OnStop();
                }
            }
            else
                LogProvider.Warn("WorkerRole has no tasks configured.");

            LogProvider.Warn("WorkerRole has stopped.");
        }
    }
}
