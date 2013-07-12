using System;
using System.Collections.Generic;
using Common.Logging;

namespace Candor.Data
{
    /// <summary>
    /// Generates sequences for tables in which the database does not have a built in sequence capability.
    /// </summary>
    public class SequenceIdGenerator
    {
        private readonly object _sequencesLock = new object();
        private readonly ISequenceIdOptimisticSyncStore _store;
        private readonly Dictionary<String, SequenceIdStore> _sequences;
        private int _maxSyncRetries = 5;
        private Boolean _ignoreCase = false;
        private ILog _logProvider;

        /// <summary>
        /// Creates a new generator that loads all sequence schemas from the supplied store.
        /// </summary>
        /// <param name="store"></param>
        public SequenceIdGenerator(ISequenceIdOptimisticSyncStore store)
        {
            _store = store;

            var sequences = store.GetSequenceIdStores();
            _sequences = new Dictionary<String, SequenceIdStore>();
            foreach (var sequence in sequences)
                _sequences.Add(sequence.Schema.TableName, sequence);
        }
        /// <summary>
        /// Creates a new generator with only a single sequence schema.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="sequence"></param>
        public SequenceIdGenerator(ISequenceIdOptimisticSyncStore store, SequenceIdStore sequence)
        {
            _store = store;
            _sequences = new Dictionary<string, SequenceIdStore> {{sequence.Schema.TableName, sequence}};
        }
        /// <summary>
        /// Gets or sets the number of retries maximum to connect to the store to reserve a new block of Ids.
        /// </summary>
        public Int32 MaxSyncRetries
        {
            get { return _maxSyncRetries; }
            set
            {
                if (_maxSyncRetries < 0)
                    throw new ArgumentOutOfRangeException("value", value, "The value must be 0 or greater.");
                _maxSyncRetries = value;
            }
        }
        /// <summary>
        /// Gets or sets if case will be ignored when getting a series of Ids and the next Id.
        /// </summary>
        public Boolean IgnoreCase
        {
            get { return _ignoreCase; }
            set { _ignoreCase = value; }
        }
        private ILog LogProvider
        {
            get { return _logProvider ?? (_logProvider = LogManager.GetLogger(typeof (SequenceIdGenerator))); }
        }
        /// <summary>
        /// Takes the first reserved Id for this node from the sequence.  
        /// If necassary, it will reserve a new block of Ids.
        /// </summary>
        /// <returns></returns>
        public String NextId(String tableName)
        {
            var sequence = GetSequenceIdStore(tableName);
            lock (sequence.IdLock)
            {
                if (sequence.LastId == sequence.FinalCachedId || sequence.FinalCachedId == null || sequence.LastId == null)
                    RenewCachedIds(sequence);
                var nextId = sequence.LastId.LexicalIncrement(sequence.CharacterSet, _ignoreCase);
                sequence.LastId = nextId;
                return nextId;
            }
        }
        private SequenceIdStore GetSequenceIdStore(String tableName)
        {
            var sequence = _sequences[tableName];
            if (sequence == null || sequence.Schema == null)
            {
                lock (_sequencesLock)
                {
                    if (sequence == null || sequence.Schema == null)
                    {
                        sequence = _store.GetSequenceIdStore(tableName);
                        if (sequence != null)
                            _sequences[tableName] = sequence;
                    }
                }
            }
            if (sequence == null)
                throw new InvalidOperationException(String.Format("Sequence '{0}' is not defined.", tableName));
            return sequence;
        }
        private void RenewCachedIds(SequenceIdStore sequence)
        {   //sequence is already locked during this call from calling method.
            int retryCount = 0;
            while (retryCount < MaxSyncRetries + 1)
            {
                try
                {
                    var syncData = _store.GetData(sequence.Schema.TableName);
                    var lastStoredId = syncData.Data;
                    syncData.Data = syncData.Data.LexicalAdd(sequence.CharacterSet, _ignoreCase, sequence.Schema.RangeSize);
                    if (_store.TryWrite(syncData))
                    {
                        sequence.LastId = lastStoredId; //next id used will increment from here.
                        sequence.FinalCachedId = syncData.Data;
                        return;
                    }
                }
                catch (Exception aex)
                {
                    LogProvider.ErrorFormat("Failed to RenewCachedIds for table '{0}' on attempt {1} of {2}", aex,
                                            sequence.Schema.TableName, retryCount, MaxSyncRetries);
                    if (retryCount == 0 && String.IsNullOrWhiteSpace(sequence.LastId))
                        _store.InsertOrUpdate(sequence.Schema);
                    if (retryCount == MaxSyncRetries)
                        throw;
                }
                retryCount++;
            }
            LogProvider.ErrorFormat("Failed to update the OptimisticSyncStore for table '{0}' after {1} attempts.  RangeSize: {2}.",
                sequence.Schema.TableName, retryCount, sequence.Schema.RangeSize);
            throw new ApplicationException(String.Format(
                "Failed to update the OptimisticSyncStore for table '{0}' after {1} attempts.  RangeSize: {2}.", 
                sequence.Schema.TableName, retryCount, sequence.Schema.RangeSize));
        }
    }
}
