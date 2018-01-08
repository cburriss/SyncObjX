using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using SyncObjX.Exceptions;
using SyncObjX.Logging;
using SyncObjX.Util;

namespace SyncObjX.Data
{
    [DataContract]
    public class EntityBatch
    {
        private EntityBatchLoggingBehavior _loggingBehavior;

        [DataMember]
        public readonly EntityToUpdateDefinition EntityDefinition;

        [DataMember]
        public bool HasBeenProcessed = false;

        [DataMember]
        public int OrderIndex = -1;

        [DataMember]
        public BindingList<EntityRecordWithoutChange> RecordsWithoutChange = new BindingList<EntityRecordWithoutChange>();

        [DataMember]
        public BindingList<RecordToAdd> RecordsToAdd = new BindingList<RecordToAdd>();

        [DataMember]
        public BindingList<RecordToUpdate> RecordsToUpdate = new BindingList<RecordToUpdate>();

        [DataMember]
        public BindingList<RecordToDelete> RecordsToDelete = new BindingList<RecordToDelete>();

        [DataMember]
        public Dictionary<RecordKeyCombo, EntityRecord> PrimaryKeys = new Dictionary<RecordKeyCombo, EntityRecord>(new RecordKeyComboComparer());

        [DataMember]
        public Dictionary<RecordKeyCombo, EntityRecord> SecondaryKeys = new Dictionary<RecordKeyCombo, EntityRecord>(new RecordKeyComboComparer());

        [DataMember]
        public EntityBatchLoggingBehavior LoggingBehavior
        {
            get { return _loggingBehavior; }

            set
            {
                if (value == null)
                    throw new Exception("Logging behavior can not be null.");
                else
                    _loggingBehavior = value;
            }
        }

        public EntityBatch(EntityToUpdateDefinition entityDefinition)
            : this(entityDefinition, new EntityBatchLoggingBehavior()) { }

        public EntityBatch(EntityToUpdateDefinition entityDefinition, EntityBatchLoggingBehavior loggingBehavior)
        {
            if (entityDefinition == null)
                throw new Exception("Entity definition can not be null.");

            EntityDefinition = entityDefinition;

            LoggingBehavior = loggingBehavior;

            RecordsWithoutChange.ListChanged += new ListChangedEventHandler((sender, args) =>
            {
                if (args.ListChangedType == ListChangedType.ItemAdded)
                {
                    AddRecordToPrimaryKeysLookup<EntityRecordWithoutChange>(RecordsWithoutChange[args.NewIndex]);

                    AddRecordToSecondaryKeysLookup<EntityRecordWithoutChange>(RecordsWithoutChange[args.NewIndex]);
                }
                else
                    throw new EnumValueNotImplementedException<ListChangedType>(args.ListChangedType);
            });

            RecordsToAdd.ListChanged += new ListChangedEventHandler((sender, args) =>
            {
                if (args.ListChangedType == ListChangedType.ItemAdded)
                {
                    var record = RecordsToAdd[args.NewIndex];

                    if (record.PrimaryKeyValues != null && record.PrimaryKeyValues.Count > 0)
                        AddRecordToPrimaryKeysLookup<RecordToAdd>(record);
                    else
                    {
                        // if primary keys are not already added, then add to the lookup once they're generated
                        record.PrimaryKeyValues.ListChanged += new ListChangedEventHandler((s, a) =>
                            {
                                if (record.PrimaryKeyValues.Count == record.AssociatedEntityBatch.EntityDefinition.PrimaryKeyColumnNames.Count)
                                    AddRecordToPrimaryKeysLookup<RecordToAdd>(record);
                            });
                    }

                    AddRecordToSecondaryKeysLookup<RecordToAdd>(record);
                }
                else
                    throw new EnumValueNotImplementedException<ListChangedType>(args.ListChangedType);
            });

            RecordsToUpdate.ListChanged += new ListChangedEventHandler((sender, args) =>
            {
                if (args.ListChangedType == ListChangedType.ItemAdded)
                {
                    AddRecordToPrimaryKeysLookup<RecordToUpdate>(RecordsToUpdate[args.NewIndex]);

                    AddRecordToSecondaryKeysLookup<RecordToUpdate>(RecordsToUpdate[args.NewIndex]);
                }
                else
                    throw new EnumValueNotImplementedException<ListChangedType>(args.ListChangedType);
            });

            RecordsToDelete.ListChanged += new ListChangedEventHandler((sender, args) =>
            {
                if (args.ListChangedType == ListChangedType.ItemAdded)
                {
                    AddRecordToPrimaryKeysLookup<RecordToDelete>(RecordsToDelete[args.NewIndex]);

                    AddRecordToSecondaryKeysLookup<RecordToDelete>(RecordsToDelete[args.NewIndex]);
                }
                else
                    throw new EnumValueNotImplementedException<ListChangedType>(args.ListChangedType);
            });
        }

        private void AddRecordToPrimaryKeysLookup<T>(EntityRecord record)
        {
            var primaryKeyCombo = new RecordKeyCombo(record.PrimaryKeyValues.ToList());

            try
            {
                PrimaryKeys.Add(primaryKeyCombo, record);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("An item of type {0} already exists for primary keys '{1}' with value(s): {2}.",
                                                   typeof(T).Name,
                                                   StringHelper.GetDelimitedString(record.AssociatedEntityBatch.EntityDefinition.PrimaryKeyColumnNames),
                                                   primaryKeyCombo.ToString()), ex);
            }
        }

        private void AddRecordToSecondaryKeysLookup<T>(EntityRecord record)
        {
            var secondaryKeys = record.SecondaryKeyValues;

            if (secondaryKeys != null && secondaryKeys.Count > 0)
            {
                var secondaryKeyCombo = new RecordKeyCombo(secondaryKeys);

                try
                {
                    SecondaryKeys.Add(secondaryKeyCombo, record);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("An item of type {0} already exists for secondary keys '{1}' with value(s): {2}.",
                                                   typeof(T).Name,
                                                   StringHelper.GetDelimitedString(record.AssociatedEntityBatch.EntityDefinition.PrimaryKeyColumnNames),
                                                   secondaryKeyCombo.ToString()), ex);
                }
            }
        }

        public List<Exception> GetExceptions()
        {
            List<Exception> exceptions = new List<Exception>();

            var insertsWithExceptions = RecordsToAdd.Where(d => d.Exception != null);

            foreach (var insertWithError in insertsWithExceptions)
            {
                exceptions.Add(insertWithError.Exception);
            }

            var updatesWithExceptions = RecordsToUpdate.Where(d => d.Exception != null);

            foreach (var updateWithError in updatesWithExceptions)
            {
                exceptions.Add(updateWithError.Exception);
            }

            var deletionsWithExceptions = RecordsToDelete.Where(d => d.Exception != null);

            foreach (var deletionWithError in deletionsWithExceptions)
            {
                exceptions.Add(deletionWithError.Exception);
            }

            return exceptions;
        }

        public override string ToString()
        {
            return string.Format("{0} - I:{1:N0}; U:{2:N0}; D:{3:N0}; NC:{4:N0}",
                                 EntityDefinition.UniqueRelationshipName,
                                 RecordsToAdd.Count, RecordsToUpdate.Count, RecordsToDelete.Count, RecordsWithoutChange.Count);
        }
    }
}
