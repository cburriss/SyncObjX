using System;
using System.Runtime.Serialization;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public abstract class SyncObject
    {
        protected Guid _id;

        protected DateTime? _createdDate;

        protected DateTime? _updatedDate;

        [DataMember]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [DataMember]
        public DateTime? CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }

        [DataMember]
        public DateTime? UpdatedDate
        {
            get { return _updatedDate; }
            set { _updatedDate = value; }
        }
    }
}
