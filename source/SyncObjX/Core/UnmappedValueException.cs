using System;

namespace SyncObjX.Core
{
    [Serializable]
    public class UnmappedValueException : ApplicationException
    {
        public SyncDirection SyncDirection;

        public string ValueWithoutMap;

        public UnmappedValueException(SyncDirection syncDirection, string valueWithoutMap)
            : base("A value was not found within the field map.")
        {
            SyncDirection = syncDirection;

            ValueWithoutMap = valueWithoutMap;
        }
    }
}
