using System;
using SyncObjX.SyncObjects;

namespace SyncObjX.Management
{
    public class ConcurrentThreadUsageChangedArgs : EventArgs
    {
        public readonly Integration Integration;

        public readonly ConcurrentThreadState NewConcurrentThreadState;

        public ConcurrentThreadUsageChangedArgs(Integration integration, ConcurrentThreadState newConcurrentThreadState)
        {
            Integration = integration;

            NewConcurrentThreadState = newConcurrentThreadState;
        }
    }
}
