using System;

namespace SyncObjX.Management
{
    public class JobStepQueueStatusChangedArgs : EventArgs
    {
        public readonly JobInstance AssociatedJobInstance;

        public readonly JobStepInstance UpdatedJobStepInstance;

        public JobStepQueueStatusChangedArgs(JobInstance associatedJobInstance, JobStepInstance updatedJobStepInstance)
        {
            AssociatedJobInstance = associatedJobInstance;

            UpdatedJobStepInstance = updatedJobStepInstance;
        }
    }
}



