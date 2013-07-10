using System;

namespace Candor.Data
{
    /// <summary>
    /// Defines a data store that can get and then update data optimistically; 
    /// Ensuring on write that it is writing over the same data it retrieved, 
    /// meaning no other process has updated the value since it was retrieved.
    /// </summary>
    public interface IOptimisticSyncStore
    {
        /// <summary>
        /// Gets the data for a specific logical group container, such as a 
        /// database table, blob, queue, or file.
        /// </summary>
        /// <param name="tableName">The table or container name.</param>
        /// <returns>The current data value and syncronization data.</returns>
        /// <exception cref="ArgumentException">An exception occurs if the table name does not exist.</exception>
        OptimisticSyncData GetData(String tableName);
        /// <summary>
        /// Attempts to write to the specific container a given data value.
        /// </summary>
        /// <param name="syncData">The new data value and synchronization key.</param>
        /// <returns>True if the update succeeded, meaning no other caller 
        /// attempted to update the value since it was retrieved.</returns>
        /// <exception cref="ArgumentException">An exception occurs if the table name does not exist.</exception>
        Boolean TryWrite(OptimisticSyncData syncData);
    }
}
