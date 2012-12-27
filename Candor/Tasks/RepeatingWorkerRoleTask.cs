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
    public abstract class RepeatingWorkerRoleTask : WorkerRoleTask
    {
        private ILog LogProvider = LogManager.GetLogger(typeof(RepeatingWorkerRoleTask));
        private System.Threading.Timer mainTimer_ = null;
        private object timerLock_ = new object();
        private object iterationLock_ = new object();
        private DateTime lastIterationTimestamp_ = DateTime.MinValue;
        private bool iterationRunning_ = false;

        public Boolean IsRunning { get; private set; }
        /// <summary>
        /// Gets or sets the amount of time to wait between completing rating
        /// one group of activities, and starting the next group.
        /// </summary>
        public double WaitingPeriodSeconds { get; set; }

        public RepeatingWorkerRoleTask()
        {
            IsRunning = false;
            WaitingPeriodSeconds = 0.0;
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
            LogProvider.WarnFormat("'{0}' is starting.", this.Name);

            try
            {
                if (WaitingPeriodSeconds < 1)
                {
                    LogProvider.WarnFormat("'{0}' is configured to never run (WaitingPeriodSeconds must be at least 1)", this.Name);
                    return;
                }
                IsRunning = true;
                lastIterationTimestamp_ = DateTime.Now;
                StartTimer();
                LogProvider.InfoFormat("'{0}' has started.", this.Name);
            }
            catch (Exception ex)
            {
                LogProvider.ErrorFormat("Unhandled exception starting task '{0}'", ex, this.Name);
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
                    iterationRunning_ = true;
                    OnWaitingPeriodElapsed();
                    lastIterationTimestamp_ = DateTime.Now;
                    iterationRunning_ = false;
                }
                catch (Exception ex)
                {
                    LogProvider.ErrorFormat("Unhandled exception executing task '{0}'", ex, this.Name);
                }
                finally
                {
                    try
                    {
                        iterationRunning_ = false;
                        ResumeTimer();
                    }
                    catch (Exception ex)
                    {
                        LogProvider.ErrorFormat("Unhandled exception resuming task '{0}'", ex, this.Name);
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
            LogProvider.InfoFormat("'{0}' has stopped.", this.Name);
        }
        /// <summary>
        /// Pings the task to ensure that it is running.
        /// </summary>
        public override void Ping()
        {
            if (iterationRunning_)
            {
                LogProvider.DebugFormat("Running iteration for '{1}' now, previous completed was {0:yyyy-MM-dd HH:mm:ss}", lastIterationTimestamp_, this.Name);
            }
            if (lastIterationTimestamp_ < DateTime.Now.AddSeconds((WaitingPeriodSeconds + 5) * -1))
            {
                LogProvider.WarnFormat("Task timer is being restarted for '{1}' due to last iteration being too old: {0:yyyy-MM-dd HH:mm:ss}", lastIterationTimestamp_, this.Name);
                ResetTimer();
            }
            else
                LogProvider.DebugFormat("Last completed iteration for '{1}' was {0:yyyy-MM-dd HH:mm:ss}", lastIterationTimestamp_, this.Name);
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
                TimeSpan duration = TimeSpan.FromSeconds(Math.Max(1, WaitingPeriodSeconds));
                mainTimer_.Change(duration, duration);
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
                LogProvider.ErrorFormat("Timer could not be cleared for task '{0}'", ex, this.Name);
            }
        }
        private void StartTimer()
        {
            try
            {
                lock (timerLock_)
                {
                    TimeSpan duration = TimeSpan.FromSeconds(Math.Max(1, WaitingPeriodSeconds));
                    mainTimer_ = new System.Threading.Timer(new TimerCallback(this.mainTimer__Elapsed), null, duration, duration);
                }
            }
            catch (Exception ex)
            {
                LogProvider.ErrorFormat("Timer could not be started for task '{0}'", ex, this.Name);
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
        /// <summary>
        /// The code to be executed everytime the waiting period elapses.
        /// </summary>
        /// <remarks>
        /// This will complete before the waiting period until the next iteration begins.
        /// </remarks>
        public abstract void OnWaitingPeriodElapsed();
    }
}
