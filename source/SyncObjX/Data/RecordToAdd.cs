using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SyncObjX.Util;

namespace SyncObjX.Data
{
    [DataContract]
    public class RecordToAdd : EntityRecordWithDataChange
    {
        [DataMember]
        public readonly Dictionary<string, string> FieldValuePairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public RecordToAdd(EntityBatch associatedEntityBatch)
            : base(associatedEntityBatch, null, null) { }

        public RecordToAdd(EntityBatch associatedEntityBatch, string primaryKeyValue)
            : base(associatedEntityBatch, new List<string>() { primaryKeyValue }, null) { }

        public RecordToAdd(EntityBatch associatedEntityBatch, List<string> primaryKeyValues)
            : base(associatedEntityBatch, primaryKeyValues, null) { }

        public RecordToAdd(EntityBatch associatedEntityBatch, List<string> primaryKeyValues, List<string> secondaryKeyValues)
            : base(associatedEntityBatch, primaryKeyValues, secondaryKeyValues) { }

        public void AddFieldValuePair(string fieldName, string value)
        {
            if (FieldValuePairs.ContainsKey(fieldName))
                throw new Exception(string.Format("Field with name '{0}' already exists.", fieldName));
            else
                FieldValuePairs.Add(fieldName, value);
        }

        public override string ToString()
        {
            return string.Format("'{0}'", StringHelper.GetDelimitedString(base.PrimaryKeyValues, "','"));
        }
    }
}
