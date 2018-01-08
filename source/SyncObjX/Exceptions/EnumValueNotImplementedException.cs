using System;

namespace SyncObjX.Exceptions
{
    [Serializable]
    public class EnumValueNotImplementedException<T> : Exception
    {
        public EnumValueNotImplementedException(object value)
            : base(string.Format("{0} '{1}' is not implemented.", typeof(T).Name, Enum.GetName(typeof(T), value))) { }
    }
}