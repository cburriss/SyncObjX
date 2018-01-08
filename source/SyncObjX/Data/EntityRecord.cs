using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace SyncObjX.Data
{
    [DataContract(IsReference = true)]
    public abstract class EntityRecord
    {
        [DataMember]
        public EntityBatch AssociatedEntityBatch;

        [DataMember]
        public BindingList<string> PrimaryKeyValues = new BindingList<string>();

        [DataMember]
        public List<string> SecondaryKeyValues = new List<string>();

        [DataMember]
        public Dictionary<string, string> DataOnlyValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        [DataMember]
        public Dictionary<string, EntityRecord> Parents = new Dictionary<string, EntityRecord>();

        [DataMember]
        public List<KeyValuePair<string, EntityRecord>> Children = new List<KeyValuePair<string, EntityRecord>>();

        public EntityRecord(EntityBatch associatedEntityBatch)
            : this(associatedEntityBatch, null, null) { }

        public EntityRecord(EntityBatch associatedEntityBatch, string primaryKeyValue)
            : this(associatedEntityBatch, new List<string>() { primaryKeyValue }, null) { }

        public EntityRecord(EntityBatch associatedEntityBatch, List<string> primaryKeyValues)
            : this(associatedEntityBatch, primaryKeyValues, null) { }

        public EntityRecord(EntityBatch associatedEntityBatch, List<string> primaryKeyValues, List<string> secondaryKeyValues)
        {
            if (associatedEntityBatch == null)
                throw new Exception("Associated entity batch can not be null.");

            AssociatedEntityBatch = associatedEntityBatch;

            if (primaryKeyValues != null)
            {
                foreach (var primaryKeyValue in primaryKeyValues)
                {
                    PrimaryKeyValues.Add(primaryKeyValue);
                }
            }

            if (secondaryKeyValues != null)
                SecondaryKeyValues = secondaryKeyValues;
        }

        public void AssociateParent(string relationshipName, EntityRecord parent)
        {
            Parents.Add(relationshipName, parent);

            parent.Children.Add(new KeyValuePair<string, EntityRecord>(relationshipName, this));
        }

        public void AssociateChild(string relationshipName, EntityRecord child)
        {
            Children.Add(new KeyValuePair<string, EntityRecord>(relationshipName, child));

            child.Parents.Add(relationshipName, this);
        }

        public string GetDataOnlyValuesAsText()
        {
            var dataOnlyFieldsText = new StringBuilder();
	
	        foreach (var kvp in DataOnlyValues)
	        {
		        dataOnlyFieldsText.AppendFormat("{0}: {1}", kvp.Key, kvp.Value == null ? "NULL" : "'" + kvp.Value + "'");
		        dataOnlyFieldsText.Append("; ");
	        }

            dataOnlyFieldsText.Remove(dataOnlyFieldsText.Length - 2, 2);

            return dataOnlyFieldsText.ToString();
        }
    }
}
