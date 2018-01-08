using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SyncObjX.Management
{
    [DataContract]
    public class JobStepInstanceResult
    {
        [DataMember]
        public Guid JobInstanceId;

        [DataMember]
        public Guid JobStepInstanceId;

        [DataMember]
        public Guid JobStepId;

        [DataMember]
        public JobStepQueueStatus Status;

        [DataMember]
        public byte OrderIndex;

        [DataMember]
        public DateTime? ActualStartTime;

        [DataMember]
        public DateTime? ActualEndTime;

        [DataMember]
        public TimeSpan? ActualDuration;

        [DataMember]
        public bool HasRecordErrors;

        [DataMember]
        public bool HasRuntimeErrors;

        [DataMember]
        public List<WebServiceException> Exceptions;
    }
}
