using System;

namespace SyncObjX.Management
{
    public class JobQueueStatusChangedArgs : EventArgs
    {
        public readonly JobInstance UpdatedJobInstance;

        public JobQueueStatusChangedArgs(JobInstance updatedJobInstance)
        {
            UpdatedJobInstance = updatedJobInstance;
        }
    }
}
