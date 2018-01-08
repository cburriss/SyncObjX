using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SyncObjX.Management
{
    [KnownType(typeof(Exception))]
    [DataContract]
    public class JobInstanceResult
    {
        [DataMember]
        public Guid QueueRequestId;

        [DataMember]
        public Guid JobInstanceId;

        [DataMember]
        public Guid IntegrationId;

        [DataMember]
        public Guid JobId;

        [DataMember]
        public IEnumerable<JobFilter> Filters;

        [DataMember]
        public Guid SourceDataSourceId;

        [DataMember]
        public Guid TargetDataSourceId;

        [DataMember]
        public JobQueueStatus Status;

        [DataMember]
        public string InvocationSource;

        [DataMember]
        public JobInvocationSourceType InvocationSourceType;

        [DataMember]
        public DateTime ScheduledStartTime;

        [DataMember]
        public DateTime? ActualStartTime;

        [DataMember]
        public DateTime? ActualEndTime;

        [DataMember]
        public TimeSpan? TimeToStartDelay;

        [DataMember]
        public TimeSpan? ActualDuration;

        [DataMember]
        public bool HasRecordErrors;

        [DataMember]
        public bool HasRuntimeErrors;

        [DataMember]
        public List<WebServiceException> Exceptions;

        [DataMember]
        public IEnumerable<JobStepInstanceResult> JobStepInstanceResults;
    }
}
