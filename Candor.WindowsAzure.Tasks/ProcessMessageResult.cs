using System;

namespace Candor.WindowsAzure.Tasks
{
    /// <summary>
    /// Options for post-processing of a queued message.
    /// </summary>
    public class ProcessMessageResult
    {
        /// <summary>
        /// Whether the processing was a success.
        /// </summary>
        public Boolean Success { get; set; }
        /// <summary>
        /// An amount of time to delay the next message.
        /// </summary>
        public Double DelayNextMessageSeconds { get; set; }
    }
}