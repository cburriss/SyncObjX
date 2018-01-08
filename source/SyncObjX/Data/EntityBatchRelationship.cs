using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SyncObjX.Data
{
    [DataContract]
    public class EntityBatchRelationship
    {
        [DataMember]
        public readonly string Name;

        [DataMember]
        public readonly string ParentUniqueRelationshipName;

        [DataMember]
        public readonly RecordKeyType LinkOn;

        [DataMember]
        public readonly List<string> ChildFieldNamesToMatchOn;

        public EntityBatchRelationship(string relationshipName, string parentUniqueRelationshipName, RecordKeyType linkOn, string childFieldNameToMatchOn)
            : this(relationshipName, parentUniqueRelationshipName, linkOn, new List<string>() { childFieldNameToMatchOn }) { }

        public EntityBatchRelationship(string relationshipName, string parentUniqueRelationshipName, RecordKeyType linkOn, List<string> childFieldNamesToMatchOn)
        {
            if (String.IsNullOrWhiteSpace(relationshipName))
                throw new Exception("Relationship name is missing or empty.");

            if (String.IsNullOrWhiteSpace(parentUniqueRelationshipName))
                throw new Exception("Parent relationship name is missing or empty.");

            if (childFieldNamesToMatchOn == null || childFieldNamesToMatchOn.Count == 0)
                throw new Exception("At least one child field name is required.");

            if (childFieldNamesToMatchOn.Exists(d => String.IsNullOrWhiteSpace(d)))
                throw new Exception("One or more child field names are missing or empty.");

            Name = relationshipName;

            ParentUniqueRelationshipName = parentUniqueRelationshipName;

            LinkOn = linkOn;

            ChildFieldNamesToMatchOn = childFieldNamesToMatchOn;
        }
    }
}
