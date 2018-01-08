using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SyncObjX.Data
{
    [DataContract]
    public abstract class EntityRecordWithDataChange : EntityRecord
    {
        [DataMember]
        public string CommandText = "";

        [DataMember]
        public bool HasBeenProcessed = false;

        [DataMember]
        public bool HasWarning = false;

        [DataMember]
        public bool HasError = false;

        [DataMember]
        public string ErrorMessage;

        [DataMember]
        public Exception Exception;

        public EntityRecordWithDataChange(EntityBatch associatedEntityBatch)
            : base(associatedEntityBatch, null, null) { }

        public EntityRecordWithDataChange(EntityBatch associatedEntityBatch, string primaryKeyValue)
            : base(associatedEntityBatch, new List<string>() { primaryKeyValue }, null) { }

        public EntityRecordWithDataChange(EntityBatch associatedEntityBatch, List<string> primaryKeyValues)
            : base(associatedEntityBatch, primaryKeyValues, null) { }

        public EntityRecordWithDataChange(EntityBatch associatedEntityBatch, List<string> primaryKeyValues, List<string> secondaryKeyValues)
            : base(associatedEntityBatch, primaryKeyValues, secondaryKeyValues) { }
    }
}
