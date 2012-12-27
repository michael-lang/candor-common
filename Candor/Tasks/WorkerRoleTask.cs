using System.Configuration.Provider;

namespace Candor.Tasks
{
    /// <summary>
    /// The base (partial) implemenation for a Worker Role.
    /// </summary>
    public abstract class WorkerRoleTask : ProviderBase
    {
        /// <summary>
        /// Starts this worker roles work loop, typically with a timer, and then returns immediately.
        /// </summary>
        public abstract void OnStart();
        /// <summary>
        /// Stops the work loop and then returns when complete.  Expect the process to terminate
        /// potentially immediately after this method returns.
        /// </summary>
        public abstract void OnStop();
        /// <summary>
        /// Pings the task to ensure it is working properly.
        /// </summary>
        public abstract void Ping();
    }
}
