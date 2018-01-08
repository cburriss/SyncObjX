using System;
using System.Runtime.Serialization;

namespace SyncObjX.Logging
{
    public class EntityBatchLoggingBehavior
    {
        private int? _maxNumberOfInsertsWithoutErrorToLog = null;

        private int? _maxNumberOfUpdatesWithoutErrorToLog = null;

        private int? _maxNumberOfDeletionsWithoutErrorToLog = null;

        [DataMember]
        public int? MaxNumberOfInsertsWithoutErrorToLog
        {
            get { return _maxNumberOfInsertsWithoutErrorToLog; }

            set
            {
                if (value.HasValue && value < 0)
                    throw new Exception("0 or more must be specified for the maximum # of inserts without error to log.");

                _maxNumberOfInsertsWithoutErrorToLog = value;
            }
        }

        [DataMember]
        public int? MaxNumberOfUpdatesWithoutErrorToLog
        {
            get { return _maxNumberOfUpdatesWithoutErrorToLog; }

            set
            {
                if (value.HasValue && value < 0)
                    throw new Exception("0 or more must be specified for the maximum # of updates without error to log.");

                _maxNumberOfUpdatesWithoutErrorToLog = value;
            }
        }

        [DataMember]
        public int? MaxNumberOfDeletionsWithoutErrorToLog
        {
            get { return _maxNumberOfDeletionsWithoutErrorToLog; }

            set
            {
                if (value.HasValue && value < 0)
                    throw new Exception("0 or more must be specified for the maximum # of deletions without error to log.");

                _maxNumberOfDeletionsWithoutErrorToLog = value;
            }
        }
    }
}
