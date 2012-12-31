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
    public class WorkerRole: IDisposable
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
        ~WorkerRole()
        {
            Dispose(false);
        }

        public void OnStart()
        {
            try
            {
                if (this._disposed)
                    throw new ObjectDisposedException("WorkerRole");
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
            if (this._disposed)
                throw new ObjectDisposedException("WorkerRole");
            lock (timerLock_)
            {
                mainTimer_.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        private void ResumeTimer()
        {
            if (this._disposed)
                throw new ObjectDisposedException("WorkerRole");
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
                if (this._disposed)
                    throw new ObjectDisposedException("WorkerRole");
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

        #region IDisposable Members
        private bool _disposed = false;
        /// <summary>
        /// Disposed of resources used by this monitor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                LogProvider.Debug("Disposing worker role.");
                OnStop();
                foreach (WorkerRoleTask task in tasks_)
                {
                    if (task is IDisposable)
                        ((IDisposable)task).Dispose();
                }
                LogProvider.Debug("Disposed worker role.");
            }
            _disposed = true;
        }
        #endregion
    }
}
