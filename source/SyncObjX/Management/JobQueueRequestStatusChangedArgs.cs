using System;

namespace SyncObjX.Management
{
    public class JobQueueRequestStatusChangedArgs : EventArgs
    {
        public readonly JobQueueRequest QueueRequest;

        public JobQueueRequestStatusChangedArgs(JobQueueRequest queueRequest)
        {
            QueueRequest = queueRequest;
        }
    }
}
