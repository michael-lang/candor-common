using System;

namespace Candor.WindowsAzure.Tasks
{
    /// <summary>
    /// Status for a job that processes a queue.
    /// </summary>
    /// <remarks>
    /// Most worker tasks store at least one record with the last time the 
    /// job ran indicating success or fail with a reason.
    /// Some may also have a status record for any failed process records
    /// indicating how far they made it and with enough details to diagnose
    /// and resume with the failed step once a code or data fix is made.
    /// </remarks>
    public class JobStatus
    {
        /// <summary>
        /// The queue name that was processed.
        /// </summary>
        public String QueueName { get; set; }
        /// <summary>
        /// Optional, the identity of the relevant queue message.
        /// </summary>
        /// <remarks>
        /// Not required on many generic jobs.  Intended for cases when processing
        /// a single record is many steps and you want to resume processing on a given
        /// step and skip the steps that have already completed.
        /// </remarks>
        public String MessageId { get; set; }
        /// <summary>
        /// Optional, a step or position left off the last time the job ran.
        /// </summary>
        /// <remarks>
        /// The exact meaning varies per the needs of the worker task that writes this status.
        /// </remarks>
        /// <example>A step for processing an individual message (paired with a MessageId)</example>
        /// <example>A group number indicator custom for your job type</example>
        public Int32 MessageStep { get; set; }
        /// <summary>
        /// The date and time when the job was run.
        /// </summary>
        public DateTime RunDateTime { get; set; }
        /// <summary>
        /// An indication of success or failure.
        /// </summary>
        public Boolean Success { get; set; }
        /// <summary>
        /// The full body of the message.
        /// </summary>
        public String Message { get; set; }
        /// <summary>
        /// A logging message with reason for success or failure.
        /// </summary>
        public String LogMessage { get; set; }
    }
}
