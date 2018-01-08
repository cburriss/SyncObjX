using System;

namespace SyncObjX.Exceptions
{
    [Serializable]
    public class DerivedClassNotImplementedException<TBase> : Exception
    {
        public DerivedClassNotImplementedException(object value)
            : base(string.Format("Derived class '{0}' or its parent class inheriting from '{1}' is not implemented.", value.GetType().Name, typeof(TBase).Name)) { }
    }
}
