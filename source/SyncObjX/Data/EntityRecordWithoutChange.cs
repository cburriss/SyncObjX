using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SyncObjX.Data
{
    [DataContract]
    public class EntityRecordWithoutChange : EntityRecord
    {
        public EntityRecordWithoutChange(EntityBatch associatedEntityBatch, List<string> primaryKeyValues)
            : base (associatedEntityBatch)
        {
            if (primaryKeyValues == null || primaryKeyValues.Count == 0)
                throw new Exception("One or more primary key values are required.");

            foreach (var primaryKeyValue in primaryKeyValues)
            {
                PrimaryKeyValues.Add(primaryKeyValue);
            }
        }
    }
}
