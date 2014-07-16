using Candor.Configuration.Provider;
using Common.Logging;
using System;
using System.Collections.Specialized;
using System.Threading;

namespace Candor.Tasks
{
    /// <summary>
    /// A worker task that repeats on a specific interval.
    /// </summary>
    public abstract class RepeatingWorkerRoleTask : WorkerRoleTask, IDisposable
    {
        private static ILog _logProvider;
        private Timer _mainTimer;
        private readonly object _timerLock = new object();
        private readonly object _iterationLock = new object();
        private DateTime _lastIterationTimestamp = DateTime.MinValue;
        private bool _iterationRunning;

        /// <summary>
        /// Gets or sets the log destination for this type.  If not set, it will be automatically loaded when needed.
        /// </summary>
        public static ILog LogProvider
        {
            get { return _logProvider ?? (_logProvider = LogManager.GetLogger(typeof(RepeatingWorkerRoleTask))); }
            set { _logProvider = value; }
        }
        public Boolean IsRunning { get; private set; }
        /// <summary>
        /// Gets or sets the amount of time to wait between completing rating
        /// one group of activities, and starting the next group.
        /// </summary>
        public double WaitingPeriodSeconds { get; set; }

        protected RepeatingWorkerRoleTask()
        {
            IsRunning = false;
            WaitingPeriodSeconds = 0.0;
        }
        ~RepeatingWorkerRoleTask()
        {
            Dispose(false);
        }

        /// <summary>
        /// Initializes the provider with the specified values.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <param name="configValue">Provider specific attributes.</param>
        public override void Initialize(string name, NameValueCollection configValue)
        {
            base.Initialize(name, configValue);

            WaitingPeriodSeconds = configValue.GetDoubleValue("WaitingPeriodSeconds", 0.0);
        }

        /// <summary>
        /// Starts this worker roles work loop, typically with a timer, and then returns immediately.
        /// </summary>
        public override void OnStart()
        {
            if (_disposed)
                throw new ObjectDisposedException("WorkerRole");
            LogProvider.WarnFormat("'{0}' is starting.", Name);

            try
            {
                if (WaitingPeriodSeconds < 1)
                {
                    LogProvider.WarnFormat("'{0}' is configured to never run (WaitingPeriodSeconds must be at least 1)", Name);
                    return;
                }
                IsRunning = true;
                _lastIterationTimestamp = DateTime.Now;
                StartTimer();
                LogProvider.InfoFormat("'{0}' has started.", Name);
            }
            catch (Exception ex)
            {
                LogProvider.ErrorFormat("Unhandled exception starting task '{0}'", ex, Name);
                throw;
            }
        }
        void mainTimer__Elapsed(object sender)
        {
            lock (_iterationLock)
            {
                IterationResult result = null;
                try
                {
                    PauseTimer();
                    _iterationRunning = true;
                    result = OnWaitingPeriodElapsedAdvanced();
                    _lastIterationTimestamp = DateTime.Now;
                    _iterationRunning = false;
                }
                catch (Exception ex)
                {
                    LogProvider.ErrorFormat("Unhandled exception executing task '{0}'", ex, Name);
                }
                finally
                {
                    try
                    {
                        _iterationRunning = false;
                        ResumeTimer(result ?? new IterationResult());
                    }
                    catch (Exception ex)
                    {
                        LogProvider.ErrorFormat("Unhandled exception resuming task '{0}'", ex, Name);
                        if (IsRunning)
                            ResetTimer();
                    }
                }
            }
        }
        /// <summary>
        /// Stops the work loop and then returns when complete.  Expect the process to terminate
        /// potentially immediately after this method returns.
        /// </summary>
        public override void OnStop()
        {
            ClearTimer();
            LogProvider.InfoFormat("'{0}' has stopped.", Name);
        }
        /// <summary>
        /// Pings the task to ensure that it is running.
        /// </summary>
        public override void Ping()
        {
            if (_disposed)
                throw new ObjectDisposedException("WorkerRole");
            if (_iterationRunning)
            {
                LogProvider.DebugFormat("Running iteration for '{1}' now, previous completed was {0:yyyy-MM-dd HH:mm:ss}", _lastIterationTimestamp, Name);
            }
            if (_lastIterationTimestamp < DateTime.Now.AddSeconds((WaitingPeriodSeconds + 5) * -1))
            {
                LogProvider.WarnFormat("Task timer is being restarted for '{1}' due to last iteration being too old: {0:yyyy-MM-dd HH:mm:ss}", _lastIterationTimestamp, Name);
                ResetTimer();
            }
            else
                LogProvider.DebugFormat("Last completed iteration for '{1}' was {0:yyyy-MM-dd HH:mm:ss}", _lastIterationTimestamp, Name);
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
        private void ResumeTimer(IterationResult result)
        {
            if (_disposed)
                throw new ObjectDisposedException("WorkerRole");
            lock (_timerLock)
            {
                var waitSeconds = result.NextWaitingPeriodSeconds > 1
                    ? result.NextWaitingPeriodSeconds
                    : Math.Max(1, WaitingPeriodSeconds);
                var duration = TimeSpan.FromSeconds(waitSeconds);
                _mainTimer.Change(duration, duration);
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
                LogProvider.ErrorFormat("Timer could not be cleared for task '{0}'", ex, Name);
            }
        }
        private void StartTimer()
        {
            try
            {
                if (_disposed)
                    throw new ObjectDisposedException("RepeatingWorkerRoleTask");
                lock (_timerLock)
                {
                    TimeSpan duration = TimeSpan.FromSeconds(Math.Max(1, WaitingPeriodSeconds));
                    _mainTimer = new Timer(mainTimer__Elapsed, null, duration, duration);
                }
            }
            catch (Exception ex)
            {
                LogProvider.ErrorFormat("Timer could not be started for task '{0}'", ex, Name);
            }
        }
        private void ResetTimer()
        {
            ClearTimer();
            StartTimer();
        }
        /// <summary>
        /// The code to be executed everytime the waiting period elapses.
        /// </summary>
        /// <remarks>
        /// This will complete before the waiting period until the next iteration begins.
        /// </remarks>
        [Obsolete]
        public abstract void OnWaitingPeriodElapsed();
        /// <summary>
        /// Code to be executed everytime the waiting period elapses,
        /// but with controls to alter the flow of subsequent iterations.
        /// This gives more fine grained control to derived classes to
        /// follow different workloads on a custom schedule.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If this method is overridden, then the other OnWaitingPeriodElapsed 
        /// method will never be called by this base class.
        /// </remarks>
        public virtual IterationResult OnWaitingPeriodElapsedAdvanced()
        {
#pragma warning disable 612
            OnWaitingPeriodElapsed();
#pragma warning restore 612
            return new IterationResult {NextWaitingPeriodSeconds = 0.0};
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
                LogProvider.DebugFormat("Disposing worker task {0}.", Name);
                ClearTimer();
                LogProvider.DebugFormat("Disposed worker task {0}.", Name);
            }
            _disposed = true;
        }
        #endregion
    }
}
