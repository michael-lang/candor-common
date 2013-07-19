using System.Linq;
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
    public class WorkerRole : IDisposable
    {
        private static ILog _logProvider;
        private Timer _mainTimer;
        private readonly object _timerLock = new object();
        private readonly object _iterationLock = new object();
        private readonly ProviderCollection<WorkerRoleTask> _tasks;

        /// <summary>
        /// Gets or sets the log destination for this type.  If not set, it will be automatically loaded when needed.
        /// </summary>
        public static ILog LogProvider
        {
            get { return _logProvider ?? (_logProvider = LogManager.GetLogger(typeof(WorkerRole))); }
            set { _logProvider = value; }
        }
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
            _tasks = tasks;
        }
        ~WorkerRole()
        {
            Dispose(false);
        }

        public void OnStart()
        {
            try
            {
                if (_disposed)
                    throw new ObjectDisposedException("WorkerRole");
                if (_tasks == null || _tasks.Count == 0)
                {
                    LogProvider.Warn("WorkerRole has no tasks configured.  Exiting.");
                    OnStop();
                    return;
                }

                foreach (WorkerRoleTask task in _tasks)
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
            lock (_iterationLock)
            {
                try
                {
                    PauseTimer();
                    LogProvider.Debug("WorkerRole is still alive.");
                    foreach (WorkerRoleTask task in _tasks)
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
            if (_disposed)
                throw new ObjectDisposedException("WorkerRole");
            lock (_timerLock)
            {
                _mainTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        private void ResumeTimer()
        {
            if (_disposed)
                throw new ObjectDisposedException("WorkerRole");
            lock (_timerLock)
            {
                _mainTimer.Change(PingInterval, PingInterval);
            }
        }
        private void ClearTimer()
        {
            try
            {
                lock (_timerLock)
                {
                    if (_mainTimer != null)
                    {
                        _mainTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        _mainTimer.Dispose();
                        _mainTimer = null;
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
                if (_disposed)
                    throw new ObjectDisposedException("WorkerRole");
                lock (_timerLock)
                {
                    _mainTimer = new Timer(mainTimer__Elapsed, null, PingInterval, PingInterval);
                }
            }
            catch (Exception ex)
            {
                LogProvider.Error("WorkerRole could not start the timer.", ex);
            }
        }
        private void ResetTimer()
        {
            ClearTimer();
            StartTimer();
        }
        public void OnStop()
        {
            IsRunning = false;
            ClearTimer();
            if (_tasks != null && _tasks.Count > 0)
            {
                foreach (WorkerRoleTask task in _tasks)
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
        private bool _disposed;
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
                foreach (var task in _tasks.OfType<IDisposable>())
                {
                    (task).Dispose();
                }
                LogProvider.Debug("Disposed worker role.");
            }
            _disposed = true;
        }
        #endregion
    }
}
