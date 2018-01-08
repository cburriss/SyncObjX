using System.Runtime.Serialization;

namespace SyncObjX.Data
{
    [DataContract]
    public struct UpdateValueSet
    {
        [DataMember]
        public string OldValue;

        [DataMember]
        public string NewValue;

        public UpdateValueSet(object oldValue, object newValue)
            : this (oldValue == null ? null : oldValue.ToString(), newValue == null ? null : newValue.ToString()) { }

        public UpdateValueSet(string oldValue, string newValue)
        {
            OldValue = oldValue;

            NewValue = newValue;
        }
    }
}
