using System;
using System.Runtime.Serialization;

namespace SyncObjX.Data
{
    [DataContract]
    public class AutoBindingForeignKey
    {
        [DataMember]
        public readonly EntityBatchRelationship Relationship;

        [DataMember]
        public readonly string FieldNameToUpdate;

        [DataMember]
        public int ParentPrimaryKeyColumnIdx;

        public AutoBindingForeignKey(EntityBatchRelationship relationship, string fieldNameToUpdate, int parentPrimaryKeyColumnIdx)
        {
            if (relationship == null)
                throw new Exception("Foreign key relationship can not be null");

            if (String.IsNullOrWhiteSpace(fieldNameToUpdate))
                throw new Exception("Field name to update is missing or empty.");

            Relationship = relationship;

            FieldNameToUpdate = fieldNameToUpdate;

            ParentPrimaryKeyColumnIdx = parentPrimaryKeyColumnIdx;
        }
    }
}
