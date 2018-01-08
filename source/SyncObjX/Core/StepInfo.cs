using System;

namespace SyncObjX.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StepInfo : Attribute
    {
        public string Description;

        public StepInfoSyncDirection SyncDirection;
    }
}
