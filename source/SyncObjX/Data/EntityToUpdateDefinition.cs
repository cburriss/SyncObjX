using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using SyncObjX.Core;
using SyncObjX.Exceptions;

namespace SyncObjX.Data
{
    [DataContract]
    public class EntityToUpdateDefinition
    {
        private string technicalEntityName;

        private string userFriendlyEntityName;

        private string relationshipName;

        private object _customCommand;

        [DataMember]
        public SyncSide SyncSide;

        [DataMember]
        public bool ApplyInserts = true;

        [DataMember]
        public bool ApplyUpdates = true;

        [DataMember]
        public bool ApplyDeletions = true;

        [DataMember]
        public List<string> PrimaryKeyColumnNames = new List<string>();

        [DataMember]
        public List<string> PrimaryKeyColumnNamesWithPrefix = new List<string>();

        [DataMember]
        public readonly BindingList<string> SecondaryKeyColumnNames = new BindingList<string>();

        [DataMember]
        public PrimaryKeyGenerationType PrimaryKeyGenerationType;

        [DataMember]
        public List<AutoBindingForeignKey> AutoBindingForeignKeys = new List<AutoBindingForeignKey>();

        [DataMember]
        public readonly Dictionary<string, EntityBatchRelationship> Relationships = new Dictionary<string, EntityBatchRelationship>(StringComparer.OrdinalIgnoreCase);

        [DataMember]
        public readonly BindingList<DataOnlyField> DataOnlyFields = new BindingList<DataOnlyField>();

        [DataMember]
        public readonly BindingList<TransposeDataOnlyField> TransposeDataOnlyFields = new BindingList<TransposeDataOnlyField>();

        /// <summary>
        /// Return true to exclude.
        /// </summary>
        public Func<DataRow, bool> InsertsToExcludeFilter;

        /// <summary>
        /// Return true to exclude.
        /// </summary>
        public Func<DataRow, bool> UpdatesToExcludeFilter;

        /// <summary>
        /// Return true to exclude.
        /// </summary>
        public Func<DataRow, bool> DeletionsToExcludeFilter;

        [DataMember]
        public HashSet<string> RequiredFields
        {
            get
            {
                return new HashSet<string>(DataOnlyFields.Where(d => d.IsRequiredByJobBatch).Select(d => d.FieldName));
            }

            private set { }
        }

        [DataMember]
        public string TechnicalEntityName
        {
            get { return technicalEntityName; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Technical entity name is missing or empty.");

                var oldTechnicalEntityName = technicalEntityName;

                technicalEntityName = value;

                if (String.IsNullOrWhiteSpace(UserFriendlyEntityName) || UserFriendlyEntityName == oldTechnicalEntityName)
                    UserFriendlyEntityName = technicalEntityName;

                if (String.IsNullOrWhiteSpace(UniqueRelationshipName) || UniqueRelationshipName == oldTechnicalEntityName)
                    UniqueRelationshipName = technicalEntityName;
            }
        }

        [DataMember]
        public string UserFriendlyEntityName
        {
            get { return userFriendlyEntityName; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception(string.Format("User friendly entity name is missing or empty for entity with technical name '{0}'.", TechnicalEntityName));

                userFriendlyEntityName = value;
            }
        }

        [DataMember]
        public string UniqueRelationshipName
        {
            get { return relationshipName; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception(string.Format("Unique relationship name is missing or empty for entity with technical name '{0}'.", TechnicalEntityName));

                relationshipName = value;
            }
        }

        [DataMember]
        public object CustomCommand
        {
            get { return _customCommand; }

            set
            {
                if (PrimaryKeyGenerationType == Data.PrimaryKeyGenerationType.Custom)
                {
                    if (value == null)
                        throw new Exception(string.Format("A custom command is required for primary key generation type '{0}'.",
                                                          Enum.GetName(typeof(PrimaryKeyGenerationType), PrimaryKeyGenerationType)));

                    if (value is string && String.IsNullOrWhiteSpace(value.ToString()))
                        throw new Exception("A custom command of type string must have a value.");
                }
                else if (value != null)
                    throw new Exception(string.Format("Custom commands can only be used when primary key generation type is '{0}'.",
                                                       Enum.GetName(typeof(PrimaryKeyGenerationType), PrimaryKeyGenerationType)));

                _customCommand = value;
            }
        }

        public EntityToUpdateDefinition(SyncSide syncSide, string technicalEntityName, string primaryKeyColumnName)
            : this(syncSide, technicalEntityName, primaryKeyColumnName, PrimaryKeyGenerationType.Manual) { }

        public EntityToUpdateDefinition(SyncSide syncSide, string technicalEntityName, List<string> primaryKeyColumnNames)
            : this(syncSide, technicalEntityName, primaryKeyColumnNames, PrimaryKeyGenerationType.Manual) { }

        public EntityToUpdateDefinition(SyncSide syncSide, string technicalEntityName, string primaryKeyColumnName, PrimaryKeyGenerationType primaryKeyGenerationType)
            : this(syncSide, technicalEntityName, new List<string> { primaryKeyColumnName }, primaryKeyGenerationType) { }

        public EntityToUpdateDefinition(SyncSide syncSide, string technicalEntityName, List<string> primaryKeyColumnNames, PrimaryKeyGenerationType primaryKeyGenerationType) 
            : this(syncSide, technicalEntityName, primaryKeyColumnNames, primaryKeyGenerationType, null) { }

        public EntityToUpdateDefinition(SyncSide syncSide, string technicalEntityName, string primaryKeyColumnName, object customCommand)
            : this(syncSide, technicalEntityName, new List<string> { primaryKeyColumnName }, PrimaryKeyGenerationType.Custom, customCommand) { }

        public EntityToUpdateDefinition(SyncSide syncSide, string technicalEntityName, List<string> primaryKeyColumnNames, object customCommand)
            : this(syncSide, technicalEntityName, primaryKeyColumnNames, PrimaryKeyGenerationType.Custom, customCommand) { }

        public EntityToUpdateDefinition(SyncSide syncSide, string technicalEntityName, string primaryKeyColumnName, PrimaryKeyGenerationType primaryKeyGenerationType, object customCommand)
            : this(syncSide, technicalEntityName, new List<string> { primaryKeyColumnName }, primaryKeyGenerationType, customCommand) { }

        public EntityToUpdateDefinition(SyncSide syncSide, string technicalEntityName, List<string> primaryKeyColumnNames, PrimaryKeyGenerationType primaryKeyGenerationType, object customCommand)
        {
            SyncSide = syncSide;

            if(!(primaryKeyGenerationType == Data.PrimaryKeyGenerationType.AutoGenerate ||
                primaryKeyGenerationType == Data.PrimaryKeyGenerationType.Manual ||
                primaryKeyGenerationType == Data.PrimaryKeyGenerationType.Custom))
                throw new EnumValueNotImplementedException<PrimaryKeyGenerationType>(primaryKeyGenerationType); 

            if (primaryKeyColumnNames == null || primaryKeyColumnNames.Count == 0)
                throw new Exception("One or more primary key columns names are required.");

            foreach (var primaryKeyColumnName in primaryKeyColumnNames)
            {
                if (String.IsNullOrWhiteSpace(primaryKeyColumnName))
                    throw new Exception("Primary key column name is missing or empty.");
            }

            TechnicalEntityName = technicalEntityName.Trim();

            primaryKeyColumnNames.ForEach(d => d.Trim());
            PrimaryKeyColumnNames = primaryKeyColumnNames;

            PrimaryKeyGenerationType = primaryKeyGenerationType;

            CustomCommand = customCommand;

            if (PrimaryKeyGenerationType == PrimaryKeyGenerationType.Manual)
            {
                foreach (var primaryKeyColumnName in PrimaryKeyColumnNames)
                {
                    DataOnlyFields.Add(new DataOnlyField(primaryKeyColumnName));
                }
            }

            switch (syncSide)
            {
                case SyncSide.Source:

                    foreach (var primaryKeyColumnName in PrimaryKeyColumnNames)
                        PrimaryKeyColumnNamesWithPrefix.Add(DataTableHelper.SOURCE_PREFIX + primaryKeyColumnName);

                    break;

                case SyncSide.Target:

                    foreach (var primaryKeyColumnName in PrimaryKeyColumnNames)
                        PrimaryKeyColumnNamesWithPrefix.Add(DataTableHelper.TARGET_PREFIX + primaryKeyColumnName);

                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncSide>(syncSide);
            }

            // validate added secondary key and add as data only field
            SecondaryKeyColumnNames.ListChanged += new ListChangedEventHandler((sender, args) =>
            {
                if (args.ListChangedType == ListChangedType.ItemAdded)
                {
                    if (String.IsNullOrWhiteSpace(SecondaryKeyColumnNames[args.NewIndex]))
                        throw new Exception("Secondary key column name is missing or empty.");

                    DataOnlyFields.Add(new DataOnlyField(SecondaryKeyColumnNames[args.NewIndex], false));
                }
            });

            // validate added data only field
            DataOnlyFields.ListChanged += new ListChangedEventHandler((sender, args) =>
            {
                if (args.ListChangedType == ListChangedType.ItemAdded)
                {
                    if (String.IsNullOrWhiteSpace(DataOnlyFields[args.NewIndex].FieldName))
                        throw new Exception("Data only field name is missing or empty.");

                    // remove if a duplicate value
                    if (DataOnlyFields.Where(d => d.FieldName.Equals(DataOnlyFields[args.NewIndex].FieldName, StringComparison.OrdinalIgnoreCase)).Count() > 1)
                    {
                        var newItem = DataOnlyFields[args.NewIndex];

                        var existingItem = DataOnlyFields.Where(d => d.FieldName.Equals(DataOnlyFields[args.NewIndex].FieldName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                        // override the mapped field if a Func is specified to populate the value
                        if (existingItem.MethodToPopulateValue != null && newItem.MethodToPopulateValue == null)
                            newItem.MethodToPopulateValue = existingItem.MethodToPopulateValue;

                        if (existingItem.IsRequiredByJobBatch)
                            newItem.IsRequiredByJobBatch = true;

                        // remove the old instance now that the new one is updated
                        DataOnlyFields.Remove(existingItem);
                    }
                }
            });

            // validate added data only field
            TransposeDataOnlyFields.ListChanged += new ListChangedEventHandler((sender, args) =>
            {
                if (args.ListChangedType == ListChangedType.ItemAdded)
                {
                    if (String.IsNullOrWhiteSpace(TransposeDataOnlyFields[args.NewIndex].FieldName))
                        throw new Exception("Transpose data only field name is missing or empty.");

                    // remove if a duplicate value
                    if (TransposeDataOnlyFields.Where(d => d.FieldName.Equals(TransposeDataOnlyFields[args.NewIndex].FieldName, StringComparison.OrdinalIgnoreCase)).Count() > 1)
                    {
                        var newItem = DataOnlyFields[args.NewIndex];

                        var existingItem = TransposeDataOnlyFields.Where(d => d.FieldName.Equals(TransposeDataOnlyFields[args.NewIndex].FieldName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                        // remove the old instance now that the new one is updated
                        TransposeDataOnlyFields.Remove(existingItem);
                    }
                }
            });
        }

        /// <summary>
        /// Defaults to parent's first primary key column (idx = 0).
        /// </summary>
        public void AddAutoBindingForeignKey(string foreignKeyFieldToUpdate, string parentEntityName, RecordKeyType linkOn, string childFieldNameToMatchOn)
        {
            AddAutoBindingForeignKey(foreignKeyFieldToUpdate, 0, parentEntityName, linkOn, new List<string>() { childFieldNameToMatchOn });
        }

        /// <summary>
        /// Defaults to parent's first primary key column (idx = 0).
        /// </summary>
        public void AddAutoBindingForeignKey(string foreignKeyFieldToUpdate, string parentUniqueRelationshipName, RecordKeyType linkOn, List<string> childFieldNamesToMatchOn)
        {
            AddAutoBindingForeignKey(foreignKeyFieldToUpdate, 0, parentUniqueRelationshipName, linkOn, childFieldNamesToMatchOn);
        }

        public void AddAutoBindingForeignKey(string foreignKeyFieldToUpdate, int parentPrimaryKeyColumnIdx, string parentUniqueRelationshipName, RecordKeyType linkOn, string childFieldNameToMatchOn)
        {
            AddAutoBindingForeignKey(foreignKeyFieldToUpdate, parentPrimaryKeyColumnIdx, parentUniqueRelationshipName, linkOn, new List<string>() { childFieldNameToMatchOn });
        }

        public void AddAutoBindingForeignKey(string foreignKeyFieldToUpdate, int parentPrimaryKeyColumnIdx, string parentUniqueRelationshipName, RecordKeyType linkOn, List<string> childFieldNamesToMatchOn)
        {
            if (String.IsNullOrWhiteSpace(foreignKeyFieldToUpdate))
                throw new Exception("Foreign key field to update is missing or empty.");

            var foreignKeyRelationship = new EntityBatchRelationship("AutoBindingForeignKey_" + Guid.NewGuid().ToString(), parentUniqueRelationshipName, linkOn, childFieldNamesToMatchOn);

            AssociateParent(foreignKeyRelationship);

            AutoBindingForeignKeys.Add(new AutoBindingForeignKey(foreignKeyRelationship, foreignKeyFieldToUpdate, parentPrimaryKeyColumnIdx));
        }

        public EntityBatchRelationship AssociateParent(string parentUniqueRelationshipName, RecordKeyType linkOn, string childFieldNameToMatchOn)
        {
            return AssociateParent(parentUniqueRelationshipName, linkOn, new List<string>() { childFieldNameToMatchOn });
        }

        public EntityBatchRelationship AssociateParent(string parentUniqueRelationshipName, RecordKeyType linkOn, List<string> childFieldNamesToMatchOn)
        {
            var relationship = new EntityBatchRelationship("ParentRel_" + Guid.NewGuid().ToString(), parentUniqueRelationshipName, linkOn, childFieldNamesToMatchOn);

            AssociateParent(relationship);

            return relationship;
        }

        public void AssociateParent(EntityBatchRelationship relationship)
        {
            if (relationship == null)
                throw new Exception("Relationship can not be null.");

            if (Relationships.ContainsKey(relationship.Name))
                throw new Exception(string.Format("A relationship with name '{0}' already exists.", relationship.Name));

            Relationships.Add(relationship.Name, relationship);

            foreach (var childFieldName in relationship.ChildFieldNamesToMatchOn)
            {
                DataOnlyFields.Add(new DataOnlyField(childFieldName, isRequiredByJobBatch: false));
            }
        }

        public static bool HasAutoGeneratedPrimaryKey(DataMap map, SyncSide syncSide)
        {
            if (GetEntityDefinition(map, syncSide).PrimaryKeyGenerationType == PrimaryKeyGenerationType.AutoGenerate)
                return true;
            else
                return false;
        }

        public static bool HasCustomGeneratedPrimaryKey(DataMap map, SyncSide syncSide)
        {
            if (GetEntityDefinition(map, syncSide).PrimaryKeyGenerationType == PrimaryKeyGenerationType.Custom)
                return true;
            return false;
        }

        public static bool HasManualGeneratedPrimaryKey(DataMap map, SyncSide syncSide)
        {
            if (GetEntityDefinition(map, syncSide).PrimaryKeyGenerationType == PrimaryKeyGenerationType.Manual)
                return true;
            return false;
        }

        private static EntityToUpdateDefinition GetEntityDefinition(DataMap map, SyncSide syncSide)
        {
            EntityToUpdateDefinition entityDefinition = null;

            if (map is OneWayDataMap)
            {
                var oneWayMap = (OneWayDataMap)map;

                entityDefinition = oneWayMap.EntityToUpdateDefinition;
            }
            else if (map is OneToMany_OneWayDataMap)
            {
                var oneToManyMap = (OneToMany_OneWayDataMap)map;

                entityDefinition = oneToManyMap.EntityToUpdateDefinition;
            }
            else if (map is TwoWayDataMap)
            {
                var twoWayMap = (TwoWayDataMap)map;

                if (syncSide == SyncSide.Source)
                    entityDefinition = twoWayMap.SourceDefinition;
                else if (syncSide == SyncSide.Target)
                    entityDefinition = twoWayMap.TargetDefinition;
                else
                    throw new EnumValueNotImplementedException<SyncSide>(syncSide);
            }
            else
                throw new DerivedClassNotImplementedException<OneToOneDataMap>(map);

            return entityDefinition;
        }

        public override string ToString()
        {
            return UniqueRelationshipName;
        }
    }
}
