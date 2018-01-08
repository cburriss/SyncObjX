using System;
using System.Runtime.Serialization;

namespace SyncObjX.Management
{
    [DataContract]
    public class ConcurrentThreadState
    {
        [DataMember]
        public int MaxConcurrentThreads;

        [DataMember]
        public int ThreadsInUse;

        [DataMember]
        public bool HasAvailableThread
        {
            get
            {
                if (ThreadsInUse < MaxConcurrentThreads)
                    return true;
                else
                    return false;
            }

            private set { }
        }

        [DataMember]
        public int CountOfAvailableThreads
        {
            get
            {
                return MaxConcurrentThreads - ThreadsInUse;
            }

            private set { }
        }

        public ConcurrentThreadState(int maxConcurrentThreads)
        {
            if (maxConcurrentThreads < 1)
                throw new Exception("Max concurrent threads must be 1 or greater.");

            MaxConcurrentThreads = maxConcurrentThreads;

            ThreadsInUse = 0;
        }
    }
}
