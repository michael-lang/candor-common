using System;

namespace Candor.WindowsAzure.Tasks
{
    /// <summary>
    /// Status for a job that processes a queue.
    /// </summary>
    public class JobStatus
    {
        /// <summary>
        /// The queue name that was processed.
        /// </summary>
        public String QueueName { get; set; }
        /// <summary>
        /// The date and time when the job was run.
        /// </summary>
        public DateTime RunDateTime { get; set; }
        /// <summary>
        /// An indication of success or failure.
        /// </summary>
        public Boolean Success { get; set; }
        /// <summary>
        /// A logging message with reason for success or failure.
        /// </summary>
        public String Message { get; set; }
    }
}
