using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SyncObjX.Management
{
    [DataContract]
    public class JobQueueRequestResult
    {
        [DataMember]
        public Guid QueueRequestId;

        [DataMember]
        public Guid IntegrationId;

        [DataMember]
        public Guid JobId;

        [DataMember]
        public JobInvocationSourceType InvocationSourceType;

        [DataMember]
        public JobQueueRequestStatus Status;

        [DataMember]
        public IEnumerable<JobInstanceResult> JobInstanceResults;
    }
}
