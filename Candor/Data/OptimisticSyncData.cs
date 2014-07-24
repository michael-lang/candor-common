using System;

namespace Candor.Data
{
    /// <summary>
    /// A container to store the actual latest sequence Id reserved for a given id generator
    /// and table name.
    /// </summary>
    public class OptimisticSyncData
    {
        /// <summary>
        /// Gets or sets the table or container name.
        /// </summary>
        public String TableName { get; set; }
        /// <summary>
        /// Gets or sets the syncronization key.
        /// </summary>
        public String ConcurrencyKey { get; set; }
        /// <summary>
        /// Gets or sets the data value.
        /// </summary>
        public String Data { get; set; }
    }
}
