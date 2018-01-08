using System;

namespace SyncObjX.Management
{
    public class JobQueueManagerStatusChangedArgs : EventArgs
    {
        public readonly JobQueueManagerStatus NewStatus;

        public JobQueueManagerStatusChangedArgs(JobQueueManagerStatus newStatus)
        {
            NewStatus = newStatus;
        }
    }
}
