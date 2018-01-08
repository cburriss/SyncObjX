using System;

namespace SyncObjX.Adapters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AdapterInfo : Attribute
    {
        public string Name;

        public string Description;
    }
}