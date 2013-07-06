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
        /// <returns>The current data value.</returns>
        String GetData(String tableName);
        /// <summary>
        /// Attempts to write to the specific container a given data value.
        /// </summary>
        /// <param name="tableName">The table or container name.</param>
        /// <param name="data">The new data value.</param>
        /// <returns>True if the update succeeded, meaning no other caller 
        /// attempted to update the value since it was retrieved.</returns>
        Boolean TryWrite(String tableName, String data);
    }
}
