using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Candor.Data
{
    public interface ISequenceIdOptimisticSyncStore : IOptimisticSyncStore
    {
        /// <summary>
        /// Ensures the sequence id schema exists with the options defined.
        /// </summary>
        /// <param name="sequence">The sequence schema.</param>
        /// <remarks> 
        /// This method should assume concurrent calls to reserve new sets of Ids,
        /// and handle optimistic concurrency accordingly.
        /// </remarks>
        void InsertOrUpdate(SequenceIdSchema sequence);
        /// <summary>
        /// Gets all the current sequence definitions.
        /// </summary>
        IEnumerable<SequenceIdStore> GetSequenceIdStores();
        /// <summary>
        /// Gets a single sequence definition for a specific table.
        /// </summary>
        /// <param name="tableName">The table name the sequence targets.</param>
        /// <returns></returns>
        SequenceIdStore GetSequenceIdStore(String tableName);
    }
}
