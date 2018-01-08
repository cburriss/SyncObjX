using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SyncObjX.SyncObjects;

namespace SyncObjX.Management
{
    [DataContract]
    public class JobQueueRequest
    {
        private JobQueueRequestStatus status = JobQueueRequestStatus.Waiting;

        [DataMember]
        public readonly Guid Id;

        [DataMember]
        public readonly Integration Integration;

        [DataMember]
        public readonly Job Job;

        [DataMember]
        public readonly JobInvocationSourceType InvocationSourceType;

        public event EventHandler<JobQueueRequestStatusChangedArgs> StatusChanged;

        [DataMember]
        public JobQueueRequestStatus Status
        {
            get { return status; }

            set
            {
                bool statusHasChanged = false;

                if (status != value)
                    statusHasChanged = true;

                status = value;

                if (StatusChanged != null && statusHasChanged)
                    StatusChanged(null, new JobQueueRequestStatusChangedArgs(this));
            }
        }

        [DataMember]
        public List<JobInstance> JobInstances = new List<JobInstance>();

        public JobQueueRequest(Integration integration, Job job, JobInvocationSourceType invocationSourceType)
            : this(Guid.Empty, integration, job, invocationSourceType) { }

        public JobQueueRequest(Guid queueRequestGuid, Integration integration, Job job, JobInvocationSourceType invocationSourceType)
        {
            if (queueRequestGuid == Guid.Empty)
                Id = Guid.NewGuid();
            else
                Id = queueRequestGuid;

            if (integration == null)
                throw new Exception("Integration can not be null.");

            if (job == null)
                throw new Exception("Job can not be null.");

            Integration = integration;

            Job = job;

            InvocationSourceType = invocationSourceType;
        }

    }
}