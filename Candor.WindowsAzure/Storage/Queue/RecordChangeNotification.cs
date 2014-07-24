using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Candor.WindowsAzure.Storage.Queue
{
    /// <summary>
    /// A queue message type with just enough information to load an item after a change.
    /// Used to put on a queue for retrieval of the object for asyncronous post processing.
    /// </summary>
    [Serializable]
    public class RecordChangeNotification
    {
        /// <summary>
        /// The type of change operation completed for the item.
        /// </summary>
        public TableOperationType OperationType { get; set; }
        /// <summary>
        /// The name of the Azure table storing the changed item.
        /// </summary>
        public String TableName { get; set; }
        /// <summary>
        /// The Azure table partition key of the changed item.
        /// </summary>
        public String PartitionKey { get; set; }
        /// <summary>
        /// The Azure table row key of the changed item.
        /// </summary>
        public String RowKey { get; set; }
    }
}
