using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SyncObjX.Data;
using SyncObjX.SyncObjects;

namespace SyncObjX.Management
{
    [DataContract]
    public class JobStepInstance
    {
        private JobStepQueueStatus status;

        [DataMember]
        public readonly Guid Id;

        [DataMember]
        public readonly JobStep JobStep;

        [DataMember]
        public JobInstance AssociatedJobInstance;

        [DataMember]
        public JobStepQueueStatus Status
        {
            get
            {
                return status;
            }

            set
            {
                bool statusHasChanged = false;

                if (status != value)
                    statusHasChanged = true;

                status = value;

                if (StatusChanged != null && statusHasChanged)
                    StatusChanged(null, new JobStepQueueStatusChangedArgs(AssociatedJobInstance, this));
            }
        }

        public event EventHandler<JobStepQueueStatusChangedArgs> StatusChanged;

        [DataMember]
        public byte OrderIndex { get; private set; }

        [DataMember]
        public DateTime? ActualStartTime;

        [DataMember]
        public DateTime? ActualEndTime;

        [DataMember]
        public TimeSpan? ActualDuration
        {
            get
            {
                if (!(ActualStartTime.HasValue && ActualEndTime.HasValue))
                    return null;
                else
                    return ActualEndTime - ActualStartTime;
            }

            private set { }
        }

        [DataMember]
        public bool HasRecordErrors = false;

        [DataMember]
        public bool HasRuntimeErrors = false;

        [DataMember]
        public List<Exception> Exceptions = new List<Exception>();

        [DataMember]
        public bool? HasDeferredExecutionUntilNextStep;

        public JobBatch SourceJobBatch;

        public JobBatch TargetJobBatch;

        public JobStepInstance(JobStep jobStep, byte orderIndex)
            : this(Guid.Empty, jobStep, orderIndex) { }

        public JobStepInstance(Guid jobStepInstanceGuid, JobStep jobStep, byte orderIndex)
        {
            if (jobStep == null)
                throw new Exception("Job step can not be null.");

            if (jobStepInstanceGuid == Guid.Empty)
                Id = Guid.NewGuid();
            else
                Id = jobStepInstanceGuid;

            JobStep = jobStep;

            OrderIndex = orderIndex;
        }
    }
}
