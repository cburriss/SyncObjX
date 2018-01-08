using System;

namespace SyncObjX.Management
{
    public class JobQueuedArgs : EventArgs
    {
        public readonly JobInstance QueuedJobInstance;

        public JobQueuedArgs(JobInstance queuedJobInstance)
        {
            QueuedJobInstance = queuedJobInstance;
        }
    }
}
