using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SyncObjX.Data;
using SyncObjX.Exceptions;

namespace SyncObjX.Core
{
    public class OneToManyDataMapProcessor
    {
        public static EntityBatch Compare(OneToMany_OneWayDataMap map, DataTable sourceData, DataTable targetData)
        {
            return Compare(map, sourceData, DuplicateRowBehavior.ThrowException, targetData, DuplicateRowBehavior.ThrowException);
        }

        public static EntityBatch Compare(OneToMany_OneWayDataMap map, 
                                          DataTable sourceData, DuplicateRowBehavior sourceDataDuplicateRowBehavior,
                                          DataTable targetData, DuplicateRowBehavior targetDataDuplicateRowBehavior)
        {
            if (map == null)
                throw new Exception("Data map can not be null.");

            if (sourceData == null)
                throw new Exception("Source-side data table can not be null.");

            if (targetData == null)
                throw new Exception("Target-side data table can not be null.");

            if (map.EntityToUpdateDefinition.InsertsToExcludeFilter != null)
                throw new Exception("Exclusion filter for inserts is not supported for one-to-many data maps.");

            if (map.EntityToUpdateDefinition.UpdatesToExcludeFilter != null)
                throw new Exception("Exclusion filter for updates is not supported for one-to-many data maps.");

            if (map.EntityToUpdateDefinition.DeletionsToExcludeFilter != null)
                throw new Exception("Exclusion filter for deletions is not supported for one-to-many data maps.");

            if (map.SyncDirection == SyncDirection.SourceToTarget)
                DataTableHelper.RemoveDuplicates(sourceData, map.JoinKeysCollection.JoinFields.Select(d => d.SourceJoinField).ToList(), sourceDataDuplicateRowBehavior);
            else if (map.SyncDirection == SyncDirection.TargetToSource)
                DataTableHelper.RemoveDuplicates(targetData, map.JoinKeysCollection.JoinFields.Select(d => d.TargetJoinField).ToList(), targetDataDuplicateRowBehavior);
            else
                throw new EnumValueNotImplementedException<SyncDirection>(map.SyncDirection);

            //ValidateSyncFieldsExistInDataTables(map, sourceData, targetData);

            //ValidateDataOnlyFieldsExistInMap(map);

            DataTable oneSideData = null;
            List<string> oneSideJoinFields = null;

            DataTable manySideData = null;
            List<string> manySideJoinFields = null;

            var batch = new EntityBatch(map.EntityToUpdateDefinition);

            if (map.EntityToUpdateDefinition.SyncSide == SyncSide.Source)
            {
                if (map.SyncDirection == SyncDirection.SourceToTarget)
                {
                    throw new Exception(string.Format("{0}-side can not be updated for sync direction '{1}'.",
                                                       Enum.GetName(typeof(SyncSide), map.EntityToUpdateDefinition.SyncSide),
                                                       Enum.GetName(typeof(SyncDirection), map.SyncDirection)));
                }

                ValidateFieldsToTransposeExistInDataTable(targetData, map.ColumnNamesToTranspose);

                oneSideData = targetData;
                oneSideJoinFields = map.JoinKeysCollection.JoinFields.Select(d => d.TargetJoinField).ToList();

                manySideData = sourceData;
                manySideJoinFields = map.JoinKeysCollection.JoinFields.Select(d => d.SourceJoinField).ToList();
            }
            else if (map.EntityToUpdateDefinition.SyncSide == SyncSide.Target)
            {
                if (map.SyncDirection == SyncDirection.TargetToSource)
                {
                    throw new Exception(string.Format("{0}-side can not be updated for sync direction '{1}'.",
                                                       Enum.GetName(typeof(SyncSide), map.EntityToUpdateDefinition.SyncSide),
                                                       Enum.GetName(typeof(SyncDirection), map.SyncDirection)));
                }

                ValidateFieldsToTransposeExistInDataTable(sourceData, map.ColumnNamesToTranspose);

                oneSideData = sourceData;
                oneSideJoinFields = map.JoinKeysCollection.JoinFields.Select(d => d.SourceJoinField).ToList();

                manySideData = targetData;
                manySideJoinFields = map.JoinKeysCollection.JoinFields.Select(d => d.TargetJoinField).ToList();
            }
            else
                throw new EnumValueNotImplementedException<SyncSide>(map.EntityToUpdateDefinition.SyncSide);

            var customSetFieldsForDelete = map.CustomSetFields
                                                    .Where(d => (d.AppliesTo.HasFlag(SyncOperation.Deletes) || d.AppliesTo.HasFlag(SyncOperation.All)));

            var dataOnlyFields = batch.EntityDefinition.DataOnlyFields.ToDictionary(d => d.FieldName, d => d, StringComparer.OrdinalIgnoreCase);

            var manySideRowsLookup = GetManySideRowsDictionary(manySideJoinFields, manySideData);

            foreach (var oneSideRow in oneSideData.Rows.Cast<DataRow>())
            {
                var oneSideRecordKeyCombo = RecordKeyCombo.GetRecordKeyComboFromDataRow(oneSideRow, oneSideJoinFields);

                List<DataRow> manySideFilteredRows = null;

                if (manySideRowsLookup.ContainsKey(oneSideRecordKeyCombo))
                    manySideFilteredRows = manySideRowsLookup[oneSideRecordKeyCombo];
                else
                    manySideFilteredRows = new List<DataRow>();

                foreach (var fieldToTranspose in map.ColumnNamesToTranspose)
                {
                    var transposeResult = map.TransposeMethod(manySideFilteredRows, oneSideRow, fieldToTranspose);

                    if (transposeResult == null)
                        continue;

                    var transposeDataOnlyValues = GetTransposeDataOnlyFieldValues(map.EntityToUpdateDefinition.TransposeDataOnlyFields, oneSideRow, manySideFilteredRows, fieldToTranspose);

                    EntityRecord entityRecord;

                    if (transposeResult is TransposeResult_AddRecord && batch.EntityDefinition.ApplyInserts)
                    {
                        entityRecord = AddInsertToBatch(map, batch, dataOnlyFields, oneSideRow, transposeResult, transposeDataOnlyValues);
                    }
                    else if (transposeResult is TransposeResult_UpdateRecord && batch.EntityDefinition.ApplyUpdates)
                    {
                        entityRecord = AddUpdateToBatch(map, batch, dataOnlyFields, oneSideRow, manySideFilteredRows, transposeResult, transposeDataOnlyValues);
                    }
                    else if (transposeResult is TransposeResult_DeleteRecord && batch.EntityDefinition.ApplyDeletions)
                    {
                        entityRecord = AddDeletionToBatch(map, batch, dataOnlyFields, oneSideRow, manySideFilteredRows, transposeResult, transposeDataOnlyValues);
                    }
                    else
                        throw new DerivedClassNotImplementedException<TransposeResult>(transposeResult);
                }
            }

            return batch;
        }

        private static Dictionary<RecordKeyCombo, List<DataRow>> GetManySideRowsDictionary(List<string> manySideJoinFields, DataTable manySideData)
        {
            var manySideRowsLookup = new Dictionary<RecordKeyCombo, List<DataRow>>(new RecordKeyComboComparer());

            foreach (DataRow row in manySideData.Rows.Cast<DataRow>())
	        {
                var recordKeyCombo = RecordKeyCombo.GetRecordKeyComboFromDataRow(row, manySideJoinFields);

                if (manySideRowsLookup.ContainsKey(recordKeyCombo))
                    manySideRowsLookup[recordKeyCombo].Add(row);
                else
                    manySideRowsLookup.Add(recordKeyCombo, new List<DataRow>() { row });
	        }

            return manySideRowsLookup;
        }

        private static Dictionary<string, string> GetTransposeDataOnlyFieldValues(IEnumerable<TransposeDataOnlyField> transposeDataOnlyFields, DataRow oneSideRow, IEnumerable<DataRow> manySideFilteredRows, string fieldToTranspose)
        {
            Dictionary<string, string> transposeDataOnlyFieldValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var transposeDataOnlyField in transposeDataOnlyFields)
            {
                var dataOnlyValue = transposeDataOnlyField.MethodToPopulateValue(manySideFilteredRows, oneSideRow, fieldToTranspose);

                string dataOnlyValueAsString;

                if (dataOnlyValue == null)
                    dataOnlyValueAsString = null;
                else
                    dataOnlyValueAsString = dataOnlyValue.ToString();

                transposeDataOnlyFieldValues.Add(transposeDataOnlyField.FieldName, dataOnlyValueAsString);
            }

            return transposeDataOnlyFieldValues;
        }

        private static EntityRecord AddInsertToBatch(OneToMany_OneWayDataMap map, EntityBatch batch, Dictionary<string, DataOnlyField> dataOnlyFields, DataRow oneSideRow, TransposeResult transposeResult, Dictionary<string, string> transposeDataOnlyValues)
        {
            var transposeRecordToAdd = (TransposeResult_AddRecord)transposeResult;

            var recordToAdd = new RecordToAdd(batch);

            foreach (var fieldValuePair in transposeRecordToAdd.FieldValuePairs)
            {
                recordToAdd.FieldValuePairs.Add(fieldValuePair.Key, fieldValuePair.Value);
            }

            foreach (var transposeDataOnlyValue in transposeDataOnlyValues)
            {
                recordToAdd.DataOnlyValues.Add(transposeDataOnlyValue.Key, transposeDataOnlyValue.Value);
            }

            foreach (var dataOnlyField in dataOnlyFields)
            {
                if (!recordToAdd.DataOnlyValues.ContainsKey(dataOnlyField.Key))
                {
                    if (dataOnlyField.Value.MethodToPopulateValue == null && transposeRecordToAdd.FieldValuePairs.ContainsKey(dataOnlyField.Key))
                        recordToAdd.DataOnlyValues.Add(dataOnlyField.Key, transposeRecordToAdd.FieldValuePairs[dataOnlyField.Key]);
                    else if (dataOnlyField.Value.MethodToPopulateValue != null)
                        recordToAdd.DataOnlyValues.Add(dataOnlyField.Key, (dataOnlyField.Value.MethodToPopulateValue(oneSideRow) ?? "").ToString());
                    else
                    {
                        if (batch.EntityDefinition.SyncSide == SyncSide.Source)
                            throw new Exception(string.Format("Source-side data only field '{0}' is not mapped.", dataOnlyField.Key));
                        else if (batch.EntityDefinition.SyncSide == SyncSide.Target)
                            throw new Exception(string.Format("Target-side data only field '{0}' is not mapped.", dataOnlyField.Key));
                        else
                            throw new EnumValueNotImplementedException<SyncSide>(batch.EntityDefinition.SyncSide);
                    }
                }
            }

            var customSetFieldsForInsert = map.CustomSetFields
                                                    .Where(d => (d.AppliesTo.HasFlag(SyncOperation.Inserts) || d.AppliesTo.HasFlag(SyncOperation.All)));

            foreach (var customSetField in customSetFieldsForInsert)
            {
                var customSetValue = customSetField.CustomSetMethod(oneSideRow);

                var value = customSetValue == null ? null : customSetValue.ToString();

                recordToAdd.AddFieldValuePair(customSetField.FieldNameToUpdate, value);

                // add as data only field, if applicable
                if (dataOnlyFields.ContainsKey(customSetField.FieldNameToUpdate) &&
                    dataOnlyFields[customSetField.FieldNameToUpdate].MethodToPopulateValue == null)
                {
                    recordToAdd.DataOnlyValues.Add(customSetField.FieldNameToUpdate, value);
                }
            }

            // add primary keys, if not auto-generate or not custom
            if (EntityToUpdateDefinition.HasManualGeneratedPrimaryKey(map, batch.EntityDefinition.SyncSide))
            {
                foreach (var primarKeyColumnName in batch.EntityDefinition.PrimaryKeyColumnNames)
                {
                    recordToAdd.PrimaryKeyValues.Add(recordToAdd.DataOnlyValues[primarKeyColumnName]);
                }
            }

            // add secondary keys, if applicable
            if (batch.EntityDefinition.SecondaryKeyColumnNames != null && batch.EntityDefinition.SecondaryKeyColumnNames.Count > 0)
            {
                recordToAdd.SecondaryKeyValues = new List<string>();

                foreach (var secondaryKeyColumnName in batch.EntityDefinition.SecondaryKeyColumnNames)
                {
                    recordToAdd.SecondaryKeyValues.Add(recordToAdd.DataOnlyValues[secondaryKeyColumnName]);
                }
            }

            if (recordToAdd.FieldValuePairs.Count > 0)
                batch.RecordsToAdd.Add(recordToAdd);

            return recordToAdd;
        }

        private static EntityRecord AddUpdateToBatch(OneToMany_OneWayDataMap map, EntityBatch batch, Dictionary<string, DataOnlyField> dataOnlyFields, 
                                                     DataRow oneSideRow, IEnumerable<DataRow> manySideFilteredRows, 
                                                     TransposeResult transposeResult, Dictionary<string, string> transposeDataOnlyValues)
        {
            // False is ordered before True
            var customSetFieldsForUpdate = map.CustomSetFields
                                                    .Where(d => (d.AppliesTo.HasFlag(SyncOperation.Updates) || d.AppliesTo.HasFlag(SyncOperation.All)))
                                                    .OrderBy(d => d.OnlyApplyWithOtherChanges);

            var recordToUpdate = GetUpdateRecord(map, batch, dataOnlyFields, oneSideRow, manySideFilteredRows, transposeResult, transposeDataOnlyValues, customSetFieldsForUpdate);

            // add record to batch
            if (recordToUpdate.FieldValuePairs.Count > 0)
            {
                batch.RecordsToUpdate.Add(recordToUpdate);

                return recordToUpdate;
            }
            else if (recordToUpdate.DataOnlyValues.Count > 0)
            {
                var recordWithoutChange = new EntityRecordWithoutChange(batch, recordToUpdate.PrimaryKeyValues.ToList());

                recordWithoutChange.DataOnlyValues = recordToUpdate.DataOnlyValues;

                recordWithoutChange.SecondaryKeyValues = recordToUpdate.SecondaryKeyValues;

                batch.RecordsWithoutChange.Add(recordWithoutChange);

                return recordWithoutChange;
            }
            else
                return null;
        }

        private static RecordToUpdate GetUpdateRecord(OneToMany_OneWayDataMap map, EntityBatch batch, Dictionary<string, DataOnlyField> dataOnlyFields, 
                                                      DataRow oneSideRow, IEnumerable<DataRow> manySideFilteredRows, 
                                                      TransposeResult transposeResult, Dictionary<string, string> transposeDataOnlyValues, 
                                                      IEnumerable<CustomSetField> customSetFieldsForUpdate)
        {
            var transposeRecordToUpdate = (TransposeResult_UpdateRecord)transposeResult;

            var recordToUpdate = new RecordToUpdate(batch, transposeRecordToUpdate.PrimaryKeyValues);

            foreach (var fieldValuePair in transposeRecordToUpdate.FieldValuePairs)
            {
                string oldValueAsString;

                var oldValue = transposeRecordToUpdate.AssociatedManySideDataRow[fieldValuePair.Key];

                if (oldValue == null || oldValue == DBNull.Value)
                    oldValueAsString = null;
                else
                    oldValueAsString = oldValue.ToString();

                if (!FieldComparer.AreEqualOfUnknownType(oldValueAsString, fieldValuePair.Value))
                {
                    var oldAndNewValues = new UpdateValueSet(oldValueAsString, fieldValuePair.Value);

                    recordToUpdate.AddFieldValuePair(fieldValuePair.Key, oldAndNewValues);
                }
            }

            foreach (var transposeDataOnlyValue in transposeDataOnlyValues)
            {
                recordToUpdate.DataOnlyValues.Add(transposeDataOnlyValue.Key, transposeDataOnlyValue.Value);
            }

            foreach (var dataOnlyField in dataOnlyFields)
            {
                if (!recordToUpdate.DataOnlyValues.ContainsKey(dataOnlyField.Key))
                {
                    if (dataOnlyField.Value.MethodToPopulateValue == null && transposeRecordToUpdate.FieldValuePairs.ContainsKey(dataOnlyField.Key))
                        recordToUpdate.DataOnlyValues.Add(dataOnlyField.Key, transposeRecordToUpdate.FieldValuePairs[dataOnlyField.Key]);
                    else if (dataOnlyField.Value.MethodToPopulateValue != null)
                        recordToUpdate.DataOnlyValues.Add(dataOnlyField.Key, (dataOnlyField.Value.MethodToPopulateValue(oneSideRow) ?? "").ToString());
                    else
                    {
                        if (batch.EntityDefinition.SyncSide == SyncSide.Source)
                            throw new Exception(string.Format("Source-side data only field '{0}' is not mapped.", dataOnlyField.Key));
                        else if (batch.EntityDefinition.SyncSide == SyncSide.Target)
                            throw new Exception(string.Format("Target-side data only field '{0}' is not mapped.", dataOnlyField.Key));
                        else
                            throw new EnumValueNotImplementedException<SyncSide>(batch.EntityDefinition.SyncSide);
                    }
                }
            }

            foreach (var customSetField in customSetFieldsForUpdate)
            {
                if (customSetField.OnlyApplyWithOtherChanges == false ||
                    recordToUpdate.FieldValuePairs.Count > 0)
                {
                    string oldValueAsString;

                    var oldValue = transposeRecordToUpdate.AssociatedManySideDataRow[customSetField.FieldNameToCompare];

                    if (oldValue == null || oldValue == DBNull.Value)
                        oldValueAsString = null;
                    else
                        oldValueAsString = oldValue.ToString();

                    var customSetValue = customSetField.CustomSetMethod(oneSideRow);

                    var oldAndNewValues = new UpdateValueSet(oldValueAsString, customSetValue == null ? null : customSetValue.ToString());

                    if (!FieldComparer.AreEqualOfUnknownType(oldValueAsString, customSetValue))
                    {
                        recordToUpdate.AddFieldValuePair(customSetField.FieldNameToUpdate, oldAndNewValues);
                    }

                    // add as data only field, if applicable
                    if (dataOnlyFields.ContainsKey(customSetField.FieldNameToUpdate) &&
                        dataOnlyFields[customSetField.FieldNameToUpdate].MethodToPopulateValue == null)
                    {
                        recordToUpdate.DataOnlyValues.Add(customSetField.FieldNameToUpdate, oldAndNewValues.NewValue);
                    }
                }
            }

            // add secondary keys, if applicable
            AddSecondaryKeyValues(map.EntityToUpdateDefinition, ref recordToUpdate);

            return recordToUpdate;
        }

        private static EntityRecord AddDeletionToBatch(OneToMany_OneWayDataMap map, EntityBatch batch, Dictionary<string, DataOnlyField> dataOnlyFields,
                                                       DataRow oneSideRow, IEnumerable<DataRow> manySideFilteredRows, 
                                                       TransposeResult transposeResult, Dictionary<string, string> transposeDataOnlyValues)
        {
            var transposeRecordToDelete = (TransposeResult_DeleteRecord)transposeResult;

            var recordToDelete = new RecordToDelete(batch, transposeRecordToDelete.PrimaryKeyValues);

            foreach (var transposeDataOnlyValue in transposeDataOnlyValues)
            {
                recordToDelete.DataOnlyValues.Add(transposeDataOnlyValue.Key, transposeDataOnlyValue.Value);
            }

            foreach (var dataOnlyField in dataOnlyFields)
            {
                if (!recordToDelete.DataOnlyValues.ContainsKey(dataOnlyField.Key))
                {
                    if (dataOnlyField.Value.MethodToPopulateValue == null)
                        recordToDelete.DataOnlyValues.Add(dataOnlyField.Key, (transposeRecordToDelete.AssociatedManySideDataRow[dataOnlyField.Key] ?? "").ToString());
                    else if (dataOnlyField.Value.MethodToPopulateValue != null)
                        recordToDelete.DataOnlyValues.Add(dataOnlyField.Key, (dataOnlyField.Value.MethodToPopulateValue(oneSideRow) ?? "").ToString());
                }
            }

            // add primary keys, if not auto-generate or not custom
            if (EntityToUpdateDefinition.HasManualGeneratedPrimaryKey(map, batch.EntityDefinition.SyncSide))
            {
                foreach (var primarKeyColumnName in batch.EntityDefinition.PrimaryKeyColumnNames)
                {
                    recordToDelete.PrimaryKeyValues.Add(recordToDelete.DataOnlyValues[primarKeyColumnName]);
                }
            }

            batch.RecordsToDelete.Add(recordToDelete);

            var customSetFieldsForDelete = map.CustomSetFields.Where(d => d.AppliesTo.HasFlag(SyncOperation.Deletes) || d.AppliesTo.HasFlag(SyncOperation.All));

            if (customSetFieldsForDelete.Count() > 0)
            {
                var transposeResultForUpdate = new TransposeResult_UpdateRecord(transposeRecordToDelete.AssociatedManySideDataRow, transposeRecordToDelete.PrimaryKeyValues, null);

                var recordToUpdate = GetUpdateRecord(map, batch, dataOnlyFields, oneSideRow, manySideFilteredRows, transposeResultForUpdate, transposeDataOnlyValues, customSetFieldsForUpdate: customSetFieldsForDelete);

                if (recordToUpdate.FieldValuePairs.Count > 0)
                    batch.RecordsToUpdate.Add(recordToUpdate);
            }

            return recordToDelete;
        }

        private static IEnumerable<DataRow> GetFilteredManySideRows(List<string> oneSideJoinFields, List<string> manySideJoinFields, DataView manySideDataView, DataRow oneSideRecord)
        {
            List<DataFilter> filters = new List<DataFilter>();

            for (int i = 0; i < manySideJoinFields.Count; i++)
            {
                filters.Add(new DataFilter(manySideJoinFields[i], DataFilterOperator.Equals, oneSideRecord[oneSideJoinFields[i]]));
            }

            manySideDataView.RowFilter = DataFilterHelper.GetSqlClause(filters);

            return manySideDataView.ToTable().Rows.Cast<DataRow>();
        }

        private static void ValidateFieldsToTransposeExistInDataTable(DataTable oneSideData, HashSet<string> fieldsToTranspose)
        {
            foreach (var fieldToTranspose in fieldsToTranspose)
            {
                if (!oneSideData.Columns.Contains(fieldToTranspose))
                    throw new Exception(string.Format("Data to transpose does not include a column with name '{0}'.", fieldToTranspose));
            }
        }

        private static void AddSecondaryKeyValues(EntityToUpdateDefinition entityToUpdateDefinition, ref RecordToUpdate recordToUpdate)
        {
            var secondaryKeyColumnNames = entityToUpdateDefinition.SecondaryKeyColumnNames;

            if (secondaryKeyColumnNames != null && secondaryKeyColumnNames.Count > 0)
            {
                recordToUpdate.SecondaryKeyValues = new List<string>();

                foreach (var secondaryKeyColumnName in secondaryKeyColumnNames)
                {
                    recordToUpdate.SecondaryKeyValues.Add(recordToUpdate.DataOnlyValues[secondaryKeyColumnName]);
                }
            }
        }
    }
}
