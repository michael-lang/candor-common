using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Candor.Data
{
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
