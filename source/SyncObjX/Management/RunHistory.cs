using System;
using System.Runtime.Serialization;

namespace SyncObjX.Management
{
    [DataContract]
    public class RunHistory
    {
        [DataMember]
        public DateTime? LastStartTime;

        [DataMember]
        public DateTime? LastEndTime;

        [DataMember]
        public DateTime? LastStartTimeWithoutRecordErrors;

        [DataMember]
        public DateTime? LastEndTimeWithoutRecordErrors;

        [DataMember]
        public DateTime? LastStartTimeWithoutRuntimeErrors;

        [DataMember]
        public DateTime? LastEndTimeWithoutRuntimeErrors;

        public RunHistory(DateTime? lastStartTime, DateTime? lastEndTime, 
                          DateTime? lastStartTimeWithoutRecordErrors, DateTime? lastEndTimeWithoutRecordErrors,
                          DateTime? lastStartTimeWithoutRuntimeErrors, DateTime? lastEndTimeWithoutRuntimeErrors)
        {
            LastStartTime = lastStartTime;

            LastEndTime = lastEndTime;

            LastStartTimeWithoutRecordErrors = lastStartTimeWithoutRecordErrors;

            LastEndTimeWithoutRecordErrors = lastEndTimeWithoutRecordErrors;

            LastStartTimeWithoutRuntimeErrors = lastStartTimeWithoutRuntimeErrors;

            LastEndTimeWithoutRuntimeErrors = lastEndTimeWithoutRuntimeErrors;
        }
    }
}
