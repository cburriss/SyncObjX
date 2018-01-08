using System;

namespace SyncObjX.Management
{
    public class JobDequeuedArgs : EventArgs
    {
        public readonly JobInstance DequeuedJobInstance;

        public JobDequeuedArgs(JobInstance dequeuedJobInstance)
        {
            DequeuedJobInstance = dequeuedJobInstance;
        }
    }
}
