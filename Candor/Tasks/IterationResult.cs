namespace Candor.Tasks
{
    /// <summary>
    /// Custom options to control the repeating operation flow from
    /// a result of an iteration.
    /// </summary>
    public class IterationResult
    {
        /// <summary>
        /// A custom amount of time to resume the next iteration.
        /// Return 0 to default to the configured WaitingPeriodSeconds of this task.
        /// </summary>
        public double NextWaitingPeriodSeconds { get; set; }
    }
}