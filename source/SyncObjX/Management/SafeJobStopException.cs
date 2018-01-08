using System;

namespace SyncObjX.Management
{
    [Serializable]
    public class SafeJobStopException : Exception
    {
        public SafeJobStopException(string message) : base(message) { }
    }
}
