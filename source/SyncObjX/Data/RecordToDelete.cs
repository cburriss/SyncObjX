using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SyncObjX.Util;

namespace SyncObjX.Data
{
    [DataContract]
    public class RecordToDelete : EntityRecordWithDataChange
    {
        [DataMember]
        public readonly Dictionary<string, UpdateValueSet> FieldValuePairs = new Dictionary<string, UpdateValueSet>(StringComparer.OrdinalIgnoreCase);

        public RecordToDelete(EntityBatch associatedEntityBatch)
            : base(associatedEntityBatch, null, null) { }

        public RecordToDelete(EntityBatch associatedEntityBatch, string primaryKeyValue)
            : base(associatedEntityBatch, new List<string>() { primaryKeyValue }, null) { }

        public RecordToDelete(EntityBatch associatedEntityBatch, List<string> primaryKeyValues)
            : base(associatedEntityBatch, primaryKeyValues, null) { }

        public RecordToDelete(EntityBatch associatedEntityBatch, List<string> primaryKeyValues, List<string> secondaryKeyValues)
            : base(associatedEntityBatch, primaryKeyValues, secondaryKeyValues) { }

        /// <summary>
        /// Sets old value to empty string.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="newValue"></param>
        public void AddFieldValuePair(string fieldName, string newValue)
        {
            AddFieldValuePair(fieldName, new UpdateValueSet("", newValue));
        }

        public void AddFieldValuePair(string fieldName, UpdateValueSet updateValue)
        {
            if (FieldValuePairs.ContainsKey(fieldName))
                throw new Exception(string.Format("Field with name '{0}' already exists.", fieldName));
            else
                FieldValuePairs.Add(fieldName, updateValue);
        }

        public override string ToString()
        {
            return string.Format("'{0}'", StringHelper.GetDelimitedString(base.PrimaryKeyValues, "','"));
        }
    }
}
