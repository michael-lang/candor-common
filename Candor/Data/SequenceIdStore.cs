using System;

namespace Candor.Data
{
    public class SequenceIdStore
    {
        private readonly object _idLock = new object();

        public object IdLock
        {
            get { return _idLock; }
        }
        /// <summary>
        /// Gets or sets the schema of this sequence store.
        /// </summary>
        public SequenceIdSchema Schema { get; set; }
        /// <summary>
        /// Gets the character set for this sequence.
        /// </summary>
        public LexicalCharacterSet CharacterSet { get { return Schema.CharacterSetType.ToCharacterSet(); }}
        /// <summary>
        /// Gets or sets the last id from which new ids should be incremented from.
        /// </summary>
        public String LastId { get; set; }
        /// <summary>
        /// Gets or sets the last id that is cached for use by the current running node (server or process).
        /// </summary>
        public String FinalCachedId { get; set; }
    }
}
