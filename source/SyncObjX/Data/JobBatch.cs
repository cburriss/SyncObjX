using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using SyncObjX.Core;
using SyncObjX.Exceptions;
using SyncObjX.SyncObjects;
using SyncObjX.Util;

namespace SyncObjX.Data
{
    [DataContract]
    public class JobBatch
    {
        [DataMember]
        public readonly SyncSide SyncSide;

        [DataMember]
        public BindingList<EntityBatch> EntityBatches = new BindingList<EntityBatch>();

        [DataMember]
        public readonly JobDataSource AssociatedDataSource;

        [DataMember]
        public bool HasRecordErrors
        {
            get
            {
                foreach (var entityBatch in EntityBatches)
                {
                    if (entityBatch.RecordsToAdd.Where(d => d.HasError).Count() > 0 ||
                        entityBatch.RecordsToUpdate.Where(d => d.HasError).Count() > 0 ||
                        entityBatch.RecordsToDelete.Where(d => d.HasError).Count() > 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            private set { }
        }

        public JobBatch(SyncSide syncSide, JobDataSource associatedDataSource)
        {
            SyncSide = syncSide;

            AssociatedDataSource = associatedDataSource ?? throw new Exception("Data source can not be null.");

            EntityBatches.ListChanged += new ListChangedEventHandler((sender, args) =>
            {
                if (args.ListChangedType == ListChangedType.ItemAdded)
                {
                    EntityBatches[args.NewIndex].OrderIndex = EntityBatches.Count;

                    var childEntityBatch = EntityBatches[args.NewIndex];

                    if (EntityBatches.Where(d => d.EntityDefinition.UniqueRelationshipName.Equals(childEntityBatch.EntityDefinition.UniqueRelationshipName, StringComparison.OrdinalIgnoreCase)).Count() > 1)
                        throw new Exception(string.Format("An entity batch with unique relationship name '{0}' already exists.", childEntityBatch.EntityDefinition.UniqueRelationshipName));

                    foreach (var relationship in childEntityBatch.EntityDefinition.Relationships)
                    {
                        var parentEntityBatch = EntityBatches.Where(d => d.EntityDefinition.UniqueRelationshipName.Equals(relationship.Value.ParentUniqueRelationshipName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                        if (parentEntityBatch == null)
                            throw new Exception(string.Format("{0}-side entity batch with unique relationship name '{1}' was not found. If the referenced entity batch is within a previous job step, ensure DeferExecutionUntilNextStep is set to true within the parent job step.", 
                                                                childEntityBatch.EntityDefinition.SyncSide, relationship.Value.ParentUniqueRelationshipName));

                        // associate child records w/o change
                        foreach (var childRecordWithoutChange in childEntityBatch.RecordsWithoutChange)
                        {
                            List<string> childKeyValues = new List<string>();

                            foreach (var childFieldName in relationship.Value.ChildFieldNamesToMatchOn)
                            {
                                try
                                {
                                    childKeyValues.Add(childRecordWithoutChange.DataOnlyValues[childFieldName]);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(string.Format("Field '{0}' not found in {1} when applying relationship '{2}'.", 
                                        childFieldName, typeof(EntityRecordWithoutChange).Name, relationship.Value.Name), ex);
                                }
                            }

                            EntityRecord parentRecord;

                            if (relationship.Value.LinkOn == RecordKeyType.PrimaryKey)
                                parentEntityBatch.PrimaryKeys.TryGetValue(new RecordKeyCombo(childKeyValues), out parentRecord);
                            else if (relationship.Value.LinkOn == RecordKeyType.SecondaryKey)
                                parentEntityBatch.SecondaryKeys.TryGetValue(new RecordKeyCombo(childKeyValues), out parentRecord);
                            else
                                throw new EnumValueNotImplementedException<RecordKeyType>(relationship.Value.LinkOn);

                            if (parentRecord != null)
                                childRecordWithoutChange.AssociateParent(relationship.Value.Name, parentRecord);
                        }

                        // associate child records to add
                        foreach (var childRecordToAdd in childEntityBatch.RecordsToAdd)
                        {
                            List<string> childKeyValues = new List<string>();

                            foreach (var childFieldName in relationship.Value.ChildFieldNamesToMatchOn)
                            {
                                try
                                {
                                    childKeyValues.Add(childRecordToAdd.DataOnlyValues[childFieldName]);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(string.Format("Field '{0}' not found in {1} when applying relationship '{2}'.",
                                        childFieldName, typeof(RecordToAdd).Name, relationship.Value.Name), ex);
                                }
                            }

                            EntityRecord parentRecord;

                            if (relationship.Value.LinkOn == RecordKeyType.PrimaryKey)
                                parentEntityBatch.PrimaryKeys.TryGetValue(new RecordKeyCombo(childKeyValues), out parentRecord);
                            else if (relationship.Value.LinkOn == RecordKeyType.SecondaryKey)
                                parentEntityBatch.SecondaryKeys.TryGetValue(new RecordKeyCombo(childKeyValues), out parentRecord);
                            else
                                throw new EnumValueNotImplementedException<RecordKeyType>(relationship.Value.LinkOn);

                            if (parentRecord != null)
                                childRecordToAdd.AssociateParent(relationship.Value.Name, parentRecord);
                        }

                        // associate child records to update
                        foreach (var childRecordToUpdate in childEntityBatch.RecordsToUpdate)
                        {
                            List<string> childKeyValues = new List<string>();

                            foreach (var childFieldName in relationship.Value.ChildFieldNamesToMatchOn)
                            {
                                try
                                {
                                    childKeyValues.Add(childRecordToUpdate.DataOnlyValues[childFieldName]);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(string.Format("Field '{0}' not found in {1} when applying relationship '{2}'.",
                                        childFieldName, typeof(RecordToUpdate).Name, relationship.Value.Name), ex);
                                }
                            }

                            EntityRecord parentRecord;

                            if (relationship.Value.LinkOn == RecordKeyType.PrimaryKey)
                                parentEntityBatch.PrimaryKeys.TryGetValue(new RecordKeyCombo(childKeyValues), out parentRecord);
                            else if (relationship.Value.LinkOn == RecordKeyType.SecondaryKey)
                                parentEntityBatch.SecondaryKeys.TryGetValue(new RecordKeyCombo(childKeyValues), out parentRecord);
                            else
                                throw new EnumValueNotImplementedException<RecordKeyType>(relationship.Value.LinkOn);

                            if (parentRecord != null)
                                childRecordToUpdate.AssociateParent(relationship.Value.Name, parentRecord);
                        }

                        // associate child records to delete
                        foreach (var childRecordToDelete in childEntityBatch.RecordsToDelete)
                        {
                            List<string> childKeyValues = new List<string>();

                            foreach (var childFieldName in relationship.Value.ChildFieldNamesToMatchOn)
                            {
                                try
                                {
                                    childKeyValues.Add(childRecordToDelete.DataOnlyValues[childFieldName]);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(string.Format("Field '{0}' not found in {1} when applying relationship '{2}'.",
                                        childFieldName, typeof(RecordToDelete).Name, relationship.Value.Name), ex);
                                }
                            }

                            EntityRecord parentRecord;

                            if (relationship.Value.LinkOn == RecordKeyType.PrimaryKey)
                                parentEntityBatch.PrimaryKeys.TryGetValue(new RecordKeyCombo(childKeyValues), out parentRecord);
                            else if (relationship.Value.LinkOn == RecordKeyType.SecondaryKey)
                                parentEntityBatch.SecondaryKeys.TryGetValue(new RecordKeyCombo(childKeyValues), out parentRecord);
                            else
                                throw new EnumValueNotImplementedException<RecordKeyType>(relationship.Value.LinkOn);

                            if (parentRecord != null)
                                childRecordToDelete.AssociateParent(relationship.Value.Name, parentRecord);
                        }
                    }
                }
                else
                    throw new EnumValueNotImplementedException<ListChangedType>(args.ListChangedType);
            });
        }

        public bool ContainsKey(string uniqueRelationshipName, List<string> secondaryKeyValues)
        {
            if (String.IsNullOrWhiteSpace(uniqueRelationshipName))
                throw new Exception("Unique relationship name is missing or empty.");

            if (secondaryKeyValues == null || secondaryKeyValues.Count == 0)
                throw new Exception("At least one secondary key values is required.");

            var entityBatch = EntityBatches.Where(d => d.EntityDefinition.UniqueRelationshipName == uniqueRelationshipName).FirstOrDefault();

            if (entityBatch == null)
                throw new Exception(string.Format("Entity batch with relationship name '{0}' does not exist.", uniqueRelationshipName));

            var secondaryKeysCombo = new RecordKeyCombo(secondaryKeyValues);

            if (entityBatch.SecondaryKeys.ContainsKey(secondaryKeysCombo))
                return true;
            else
                return false;
        }

        public EntityRecord GetEntityRecord(string uniqueRelationshipName, List<string> secondaryKeyValues)
        {
            if (String.IsNullOrWhiteSpace(uniqueRelationshipName))
                throw new Exception("Unique relationship name is missing or empty.");

            if (secondaryKeyValues == null || secondaryKeyValues.Count == 0)
                throw new Exception("At least one secondary key values is required.");

            var entityBatch = EntityBatches.Where(d => d.EntityDefinition.UniqueRelationshipName == uniqueRelationshipName).FirstOrDefault();

            if (entityBatch == null)
                throw new Exception(string.Format("Entity batch with relationship name '{0}' does not exist.", uniqueRelationshipName));

            var secondaryKeysCombo = new RecordKeyCombo(secondaryKeyValues);

            if (entityBatch.SecondaryKeys.ContainsKey(secondaryKeysCombo))
                return entityBatch.SecondaryKeys[secondaryKeysCombo];
            else
                throw new Exception(string.Format("No entity record exists for relationship name '{0}' and secondary keys '{1}'.",
                                                  uniqueRelationshipName, StringHelper.GetDelimitedString(secondaryKeyValues, "','")));
        }

        public void SubmitToDataSource()
        {
            AssociatedDataSource.DataSource.ProcessBatch(this);
        }

        public List<Exception> GetExceptions()
        {
            List<Exception> exceptions = new List<Exception>();

            foreach (var entityBatch in EntityBatches)
            {
                exceptions.AddRange(entityBatch.GetExceptions());
            }

            return exceptions;
        }
    }
}
