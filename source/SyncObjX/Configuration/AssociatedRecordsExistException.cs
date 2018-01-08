using System;

namespace SyncObjX.Configuration
{
    [Serializable]
    public class AssociatedRecordsExistException : ApplicationException
    {
        public AssociatedRecordsExistException(string message) : base(message) { }
    }
}
