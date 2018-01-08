using System;

namespace SyncObjX.Configuration
{
    [Serializable]
    public class RecordNotFoundException : ApplicationException
    {
        public RecordNotFoundException(string message) : base(message) { }
    }
}
