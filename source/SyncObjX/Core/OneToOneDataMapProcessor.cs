using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using SyncObjX.Data;
using SyncObjX.Exceptions;

namespace SyncObjX.Core
{
    public class OneToOneDataMapProcessor
    {
        public static EntityBatch Compare(OneWayDataMap map, 
                                          DataTable sourceData, DuplicateRowBehavior sourceDataDuplicateRowBehavior, 
                                          DataTable targetData, DuplicateRowBehavior targetDataDuplicateRowBehavior)
        {
            return Compare(map, sourceData, sourceDataDuplicateRowBehavior, targetData, targetDataDuplicateRowBehavior, null);
        }

        public static EntityBatch Compare(OneWayDataMap map, 
                                          DataTable sourceData, DuplicateRowBehavior sourceDataDuplicateRowBehavior, 
                                          DataTable targetData, DuplicateRowBehavior targetDataDuplicateRowBehavior,
                                          Func<DataRow, bool> rowsToProcess)
        {
            if (map == null)
                throw new Exception("Data map can not be null.");

            if (sourceData == null)
                throw new Exception("Source-side data table can not be null.");

            if (targetData == null)
                throw new Exception("Target-side data table can not be null.");

            DataTableHelper.RemoveDuplicates(sourceData, map.JoinKeysCollection.JoinFields.Select(d => d.SourceJoinField).ToList(), sourceDataDuplicateRowBehavior);
            DataTableHelper.RemoveDuplicates(targetData, map.JoinKeysCollection.JoinFields.Select(d => d.TargetJoinField).ToList(), targetDataDuplicateRowBehavior);

            ValidatateEntityToUpdateDefinition(map);

            ValidateSyncFieldsExistInDataTables(map, sourceData, targetData);

            ValidateDataOnlyFieldsExistInMap(map);

            var entityBatch = new EntityBatch(map.EntityToUpdateDefinition);

            var combinedTable = DataTableHelper.JoinTables(sourceData, targetData, map.JoinKeysCollection);

            if (!combinedTable.Columns.Contains("WinningSide"))
                combinedTable.Columns.Add("WinningSide", typeof(string));

            if (rowsToProcess != null)
                combinedTable = DataTableHelper.ApplyFilter(combinedTable, rowsToProcess);

            EntityBatch sourceEntityBatch = null;
            EntityBatch targetEntityBatch = null;

            if (map.EntityToUpdateDefinition.SyncSide == SyncSide.Source)
                sourceEntityBatch = entityBatch;
            else if (map.EntityToUpdateDefinition.SyncSide == SyncSide.Target)
                targetEntityBatch = entityBatch;
            else
                throw new EnumValueNotImplementedException<SyncSide>(map.EntityToUpdateDefinition.SyncSide);

            if (map.EntityToUpdateDefinition.ApplyInserts)
                AddInsertsToBatch(ref entityBatch, combinedTable, map, map.SyncDirection);

            if (map.EntityToUpdateDefinition.ApplyUpdates)
                AddUpdatesToBatch(ref sourceEntityBatch, ref targetEntityBatch, combinedTable, map);

            if (map.EntityToUpdateDefinition.ApplyDeletions)
                AddDeletionsToBatch(ref entityBatch, combinedTable, map);

            return entityBatch;
        }

        public static void Compare(TwoWayDataMap map, DataTable sourceData, DuplicateRowBehavior sourceDataDuplicateRowBehavior, 
                                   DataTable targetData, DuplicateRowBehavior targetDataDuplicateRowBehavior,
                                   out EntityBatch sourceEntityBatch, out EntityBatch targetEntityBatch)
        {
            Compare(map, sourceData, sourceDataDuplicateRowBehavior, targetData, targetDataDuplicateRowBehavior, 
                    null, out sourceEntityBatch, out targetEntityBatch);
        }

        public static void Compare(TwoWayDataMap map, DataTable sourceData, DuplicateRowBehavior sourceDataDuplicateRowBehavior, 
                                   DataTable targetData, DuplicateRowBehavior targetDataDuplicateRowBehavior,
                                   Func<DataRow, bool> rowsToProcess,
                                   out EntityBatch sourceEntityBatch, out EntityBatch targetEntityBatch)
        {
            if (map == null)
                throw new Exception("Data map can not be null.");

            if (sourceData == null)
                throw new Exception("Source-side data table can not be null.");

            if (targetData == null)
                throw new Exception("Target-side data table can not be null.");

            DataTableHelper.RemoveDuplicates(sourceData, map.JoinKeysCollection.JoinFields.Select(d => d.SourceJoinField).ToList(), sourceDataDuplicateRowBehavior);
            DataTableHelper.RemoveDuplicates(targetData, map.JoinKeysCollection.JoinFields.Select(d => d.TargetJoinField).ToList(), targetDataDuplicateRowBehavior);

            ValidatateEntityToUpdateDefinition(map);

            ValidateSyncFieldsExistInDataTables(map, sourceData, targetData);

            ValidateDataOnlyFieldsExistInMap(map);

            var combinedTable = DataTableHelper.JoinTables(sourceData, targetData, map.JoinKeysCollection);

            if (!combinedTable.Columns.Contains("WinningSide"))
                combinedTable.Columns.Add("WinningSide", typeof(string));

            if (rowsToProcess != null)
                combinedTable = DataTableHelper.ApplyFilter(combinedTable, rowsToProcess);

            sourceEntityBatch = new EntityBatch(map.SourceDefinition);

            targetEntityBatch = new EntityBatch(map.TargetDefinition);

            if (map.SourceDefinition.ApplyInserts)
                AddInsertsToBatch(ref sourceEntityBatch, combinedTable, map, SyncDirection.TargetToSource);

            if (map.TargetDefinition.ApplyInserts)
                AddInsertsToBatch(ref targetEntityBatch, combinedTable, map, SyncDirection.SourceToTarget);

            if (map.SourceDefinition.ApplyUpdates || map.TargetDefinition.ApplyUpdates)
                AddUpdatesToBatch(ref sourceEntityBatch, ref targetEntityBatch, combinedTable, map);

            if (map.SourceDefinition.ApplyDeletions || map.TargetDefinition.ApplyDeletions)
                throw new Exception("Deletions can not be enabled for a two-way map.");
        }

        private static void ValidatateEntityToUpdateDefinition(OneToOneDataMap map)
        {
            if (map is OneWayDataMap)
            {
                var oneWayMap = (OneWayDataMap)map;

                if (oneWayMap.EntityToUpdateDefinition.TransposeDataOnlyFields.Count > 0)
                    throw new Exception("Transpose data only fields are not allowed for one-to-one data maps.");
            }
            else if (map is TwoWayDataMap)
            {
                var twoWayMap = (TwoWayDataMap)map;

                if (twoWayMap.SourceDefinition.TransposeDataOnlyFields.Count > 0 ||
                    twoWayMap.TargetDefinition.TransposeDataOnlyFields.Count > 0)
                {
                    throw new Exception("Transpose data only fields are not allowed for one-to-one data maps.");
                }
            }
            else
                throw new DerivedClassNotImplementedException<OneToOneDataMap>(map);
        }

        private static void ValidateSyncFieldsExistInDataTables(OneToOneDataMap map, DataTable sourceData, DataTable targetData)
        {
            ValidateDataFieldsExistInDataTable(SyncSide.Source, map, sourceData);
            ValidateDataFieldsExistInDataTable(SyncSide.Target, map, targetData);
        }

        private static void ValidateDataFieldsExistInDataTable(SyncSide syncSide, OneToOneDataMap map, DataTable data, bool dataColumnNamesIncludeSyncSidePrefix = false)
        {
            if (syncSide == SyncSide.Source)
            {
                foreach (var field in map.CompareFields)
                {
                    var fieldToCompare = dataColumnNamesIncludeSyncSidePrefix ? field.SourceFieldToCompareWithPrefix : field.SourceFieldToCompare;

                    if (field.SourceFieldToUpdate != null && !data.Columns.Contains(fieldToCompare))
                        throw new Exception(string.Format("Source-side data does not include a column with name '{0}'.", fieldToCompare));
                }
            }
            else if (syncSide == SyncSide.Target)
            {
                foreach (var field in map.CompareFields)
                {
                    var fieldToCompare = dataColumnNamesIncludeSyncSidePrefix ? field.TargetFieldToCompareWithPrefix : field.TargetFieldToCompare;

                    if (field.TargetFieldToUpdate != null && !data.Columns.Contains(fieldToCompare))
                        throw new Exception(string.Format("Target-side data does not include a column with name '{0}'.", fieldToCompare));
                }
            }
            else
                throw new EnumValueNotImplementedException<SyncSide>(syncSide);

            foreach (var field in map.CustomSetFields.Where(d => d.SyncSide == syncSide))
            {
                var fieldToUpdate = dataColumnNamesIncludeSyncSidePrefix ? field.FieldNameToCompareWithPrefix : field.FieldNameToCompare;

                if (!data.Columns.Contains(fieldToUpdate))
                    throw new Exception(string.Format("{0}-side data does not include a column with name '{1}'.", 
                                                      Enum.GetName(typeof(SyncSide), syncSide), field.FieldNameToCompare));
            }
        }

        private static void ValidateDataOnlyFieldsExistInMap(OneToOneDataMap map)
        {
            if (map is OneWayDataMap)
            {
                var oneWayMap = (OneWayDataMap)map;    

                if (oneWayMap.EntityToUpdateDefinition.SyncSide == SyncSide.Source)
                {
                    foreach (var dataOnlyField in oneWayMap.EntityToUpdateDefinition.DataOnlyFields.Where(d => d.MethodToPopulateValue == null))
                    {
                        if (map.CompareFields.Where(d => d.SourceFieldToUpdate.Equals(dataOnlyField.FieldName, StringComparison.OrdinalIgnoreCase)).Count() == 0 &&
                            map.CustomSetFields.Where(d => d.SyncSide == SyncSide.Source
                                && d.FieldNameToUpdate.Equals(dataOnlyField.FieldName, StringComparison.OrdinalIgnoreCase)).Count() == 0)
                        {
                            throw new Exception(string.Format("Source-side data only field '{0}' is not mapped.", dataOnlyField.FieldName));
                        }
                    }
                }
                else if (oneWayMap.EntityToUpdateDefinition.SyncSide == SyncSide.Target)
                {
                    foreach (var dataOnlyField in oneWayMap.EntityToUpdateDefinition.DataOnlyFields.Where(d => d.MethodToPopulateValue == null))
                    {
                        if (map.CompareFields.Where(d => d.TargetFieldToUpdate.Equals(dataOnlyField.FieldName, StringComparison.OrdinalIgnoreCase)).Count() == 0 &&
                            map.CustomSetFields.Where(d => d.SyncSide == SyncSide.Target
                                && d.FieldNameToUpdate.Equals(dataOnlyField.FieldName, StringComparison.OrdinalIgnoreCase)).Count() == 0)
                        {
                            throw new Exception(string.Format("Target-side data only field '{0}' is not mapped.", dataOnlyField.FieldName));
                        }
                    }
                }
                else
                    throw new EnumValueNotImplementedException<SyncSide>(oneWayMap.EntityToUpdateDefinition.SyncSide);
            }
            else if (map is TwoWayDataMap)
            {
                var twoWayMap = (TwoWayDataMap)map;

                foreach (var dataOnlyField in twoWayMap.SourceDefinition.DataOnlyFields.Where(d => d.MethodToPopulateValue == null))
                {
                    if (map.CompareFields.Where(d => d.SourceFieldToUpdate.Equals(dataOnlyField.FieldName, StringComparison.OrdinalIgnoreCase)).Count() == 0 &&
                        map.CustomSetFields.Where(d => d.SyncSide == SyncSide.Source
                            && d.FieldNameToUpdate.Equals(dataOnlyField.FieldName, StringComparison.OrdinalIgnoreCase)).Count() == 0)
                    {
                        throw new Exception(string.Format("Source-side data only field '{0}' is not mapped.", dataOnlyField.FieldName));
                    }
                }

                foreach (var dataOnlyField in twoWayMap.TargetDefinition.DataOnlyFields.Where(d => d.MethodToPopulateValue == null))
                {
                    if (map.CompareFields.Where(d => d.TargetFieldToUpdate.Equals(dataOnlyField.FieldName, StringComparison.OrdinalIgnoreCase)).Count() == 0 &&
                        map.CustomSetFields.Where(d => d.SyncSide == SyncSide.Target
                            && d.FieldNameToUpdate.Equals(dataOnlyField.FieldName, StringComparison.OrdinalIgnoreCase)).Count() == 0)
                    {
                        throw new Exception(string.Format("Target-side data only field '{0}' is not mapped.", dataOnlyField.FieldName));
                    }
                }
            }
            else
                throw new DerivedClassNotImplementedException<OneToOneDataMap>(map);
        }

        private static void AddInsertsToBatch(ref EntityBatch batch, DataTable combinedTable, OneToOneDataMap map, SyncDirection syncDirection)
        {
            SyncSide syncSide;
            string joinFieldToCheckForNull;
            string joinFieldToCheckForNotNull;

            switch (syncDirection)
            {
                case SyncDirection.SourceToTarget:
                    syncSide = SyncSide.Target;
                    joinFieldToCheckForNull = map.JoinKeysCollection.JoinFields[0].TargetJoinFieldWithPrefix;
                    joinFieldToCheckForNotNull = map.JoinKeysCollection.JoinFields[0].SourceJoinFieldWithPrefix;
                    break;

                case SyncDirection.TargetToSource:
                    syncSide = SyncSide.Source;
                    joinFieldToCheckForNull = map.JoinKeysCollection.JoinFields[0].SourceJoinFieldWithPrefix;
                    joinFieldToCheckForNotNull = map.JoinKeysCollection.JoinFields[0].TargetJoinFieldWithPrefix;
                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncDirection>(syncDirection);
            }

            var rowsToInsert = combinedTable.AsEnumerable()
                                    .Where(row => row.Field<object>(joinFieldToCheckForNull) == null &&
                                           row.Field<object>(joinFieldToCheckForNotNull) != null);

            AddInsertsToBatch(ref batch, map, syncSide, rowsToInsert);
        }

        public static void AddInsertsToBatch(ref EntityBatch batch, OneToOneDataMap map, SyncSide syncSideToUpdate, IEnumerable<DataRow> rowsToInsert, bool dataColumnNamesIncludeSyncSidePrefix = true)
        {
            var customSetFieldsForInsert = map.CustomSetFields
                                                    .Where(d => (d.AppliesTo.HasFlag(SyncOperation.Inserts) || d.AppliesTo.HasFlag(SyncOperation.All)) &&
                                                                d.SyncSide == syncSideToUpdate);

            var dataOnlyFields = batch.EntityDefinition.DataOnlyFields.ToDictionary(d => d.FieldName, d => d, StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in rowsToInsert)
            {
                UpdateWinningSideInRow(row, batch.EntityDefinition.SyncSide == SyncSide.Source ? ConflictResolutionResult.TargetWon : ConflictResolutionResult.SourceWon);

                if (batch.EntityDefinition.InsertsToExcludeFilter != null &&
                    batch.EntityDefinition.InsertsToExcludeFilter(row))
                {
                    continue;
                }

                var recordToAdd = new RecordToAdd(batch);

                foreach (var compareField in map.CompareFields)
                {
                    string fieldToUpdate;
                    string fromValue;

                    switch (syncSideToUpdate)
                    {
                        case SyncSide.Target:

                            fieldToUpdate = compareField.TargetFieldToUpdate;

                            if (dataColumnNamesIncludeSyncSidePrefix)
                                fromValue = row[compareField.SourceFieldToCompareWithPrefix] == DBNull.Value ? null : row[compareField.SourceFieldToCompareWithPrefix].ToString();
                            else
                                fromValue = row[compareField.SourceFieldToCompare] == DBNull.Value ? null : row[compareField.SourceFieldToCompare].ToString();

                            break;

                        case SyncSide.Source:

                            fieldToUpdate = compareField.SourceFieldToUpdate;

                            if (dataColumnNamesIncludeSyncSidePrefix)
                                fromValue = row[compareField.TargetFieldToCompareWithPrefix] == DBNull.Value ? null : row[compareField.TargetFieldToCompareWithPrefix].ToString();
                            else
                                fromValue = row[compareField.TargetFieldToCompare] == DBNull.Value ? null : row[compareField.TargetFieldToCompare].ToString();

                            break;

                        default:
                            throw new EnumValueNotImplementedException<SyncSide>(syncSideToUpdate);
                    }

                    switch (compareField.Type)
                    {
                        case CompareType.ConvertToType:
                            ApplyPreSaveConversion(syncSideToUpdate, row, compareField, ref fromValue);
                            recordToAdd.AddFieldValuePair(fieldToUpdate, fromValue);
                            break;

                        case CompareType.FieldMap:

                            if (syncSideToUpdate == SyncSide.Target)
                            {
                                recordToAdd.AddFieldValuePair(fieldToUpdate,
                                    compareField.FieldMap.GetMappedValueWithExceptionThrower(fieldToUpdate, syncSideToUpdate, fromValue));
                            }
                            else
                            {
                                recordToAdd.AddFieldValuePair(fieldToUpdate,
                                    compareField.FieldMap.GetMappedValueWithExceptionThrower(fieldToUpdate, syncSideToUpdate, fromValue));
                            }

                            break;

                        case CompareType.CustomMethod:
                            ApplyPreSaveConversion(syncSideToUpdate, row, compareField, ref fromValue);
                            recordToAdd.AddFieldValuePair(fieldToUpdate, fromValue);
                            break;

                        default:
                            throw new EnumValueNotImplementedException<CompareType>(compareField.Type);
                    }

                    // add as data only field, if applicable
                    if (dataOnlyFields.ContainsKey(fieldToUpdate) &&
                        dataOnlyFields[fieldToUpdate].MethodToPopulateValue == null)
                    {
                        recordToAdd.DataOnlyValues.Add(fieldToUpdate, fromValue);
                    }
                }
                
                foreach (var dataOnlyFieldWithFunc in dataOnlyFields.Where(d => d.Value.MethodToPopulateValue != null))
                {
                    recordToAdd.DataOnlyValues.Add(dataOnlyFieldWithFunc.Key, (dataOnlyFieldWithFunc.Value.MethodToPopulateValue(row) ?? "").ToString());
                }

                foreach (var customSetField in customSetFieldsForInsert)
                {
                    var customSetValue = customSetField.CustomSetMethod(row);

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
                if (EntityToUpdateDefinition.HasManualGeneratedPrimaryKey(map, syncSideToUpdate))
                {
                    foreach (var primarKeyColumnName in batch.EntityDefinition.PrimaryKeyColumnNames)
                    {
                        recordToAdd.PrimaryKeyValues.Add(recordToAdd.DataOnlyValues[primarKeyColumnName]);
                    }
                }

                // add secondary keys, if applicable
                var winningSide = syncSideToUpdate == SyncSide.Source ? ConflictResolutionResult.TargetWon : ConflictResolutionResult.SourceWon;
                AddSecondaryKeyValues(map, winningSide, recordToAdd);

                if (recordToAdd.FieldValuePairs.Count > 0)
                    batch.RecordsToAdd.Add(recordToAdd);
            }
        }

        private static void AddUpdatesToBatch(ref EntityBatch sourceEntityBatch, ref EntityBatch targetEntityBatch, DataTable combinedTable, OneToOneDataMap map)
        {
            bool applyUpdatesToSource = false;
            bool applyUpdatesToTarget = false;

            if (map is OneWayDataMap)
            {
                var convertedMap = (OneWayDataMap)map;

                // already checked in Compare method to determine if updates should be applied for unidirectional
                if (convertedMap.SyncDirection == SyncDirection.SourceToTarget)
                    applyUpdatesToTarget = true;
                else if (convertedMap.SyncDirection == SyncDirection.TargetToSource)
                    applyUpdatesToSource = true;
                else
                    throw new EnumValueNotImplementedException<SyncDirection>(convertedMap.SyncDirection);
            }
            else if (map is TwoWayDataMap)
            {
                var convertedMap = (TwoWayDataMap)map;

                if (convertedMap.SourceDefinition.ApplyUpdates)
                    applyUpdatesToSource = true;

                if (convertedMap.TargetDefinition.ApplyUpdates)
                    applyUpdatesToTarget = true;
            }
            else
                throw new DerivedClassNotImplementedException<OneToOneDataMap>(map);

            // exit if no updates are to be applied
            if (!(applyUpdatesToSource || applyUpdatesToTarget))
                return;

            var rowsToCompare = from row in combinedTable.AsEnumerable()
                                where row.Field<object>(map.JoinKeysCollection.JoinFields[0].SourceJoinFieldWithPrefix) != null
                                && row.Field<object>(map.JoinKeysCollection.JoinFields[0].TargetJoinFieldWithPrefix) != null
                                select row;

            var sourceSideCustomSetFieldsForUpdate = map.CustomSetFields
                                                    .Where(d => !d.OnlyApplyWithOtherChanges &&
                                                                (d.AppliesTo.HasFlag(SyncOperation.Updates) || d.AppliesTo.HasFlag(SyncOperation.All)) &&
                                                                d.SyncSide == SyncSide.Source);

            var sourceSideCustomSetFieldsForUpdateWithChangesOnly = map.CustomSetFields
                                                                .Where(d => d.OnlyApplyWithOtherChanges == true &&
                                                                            (d.AppliesTo.HasFlag(SyncOperation.Updates) || d.AppliesTo.HasFlag(SyncOperation.All)) &&
                                                                d.SyncSide == SyncSide.Source);

            Dictionary<string, DataOnlyField> sourceSideDataOnlyFields = null;
            List<CompareField> sourceSideCompareFieldsForDataOnlyFields = new List<CompareField>();

            if (sourceEntityBatch != null)
            {
                sourceSideDataOnlyFields = sourceEntityBatch.EntityDefinition.DataOnlyFields.ToDictionary(d => d.FieldName, d => d, StringComparer.OrdinalIgnoreCase);

                sourceSideCompareFieldsForDataOnlyFields = map.CompareFields.Where(d => sourceSideDataOnlyFields.Keys.Contains(d.SourceFieldToUpdate, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            var targetSideCustomSetFieldsForUpdate = map.CustomSetFields
                                                    .Where(d => !d.OnlyApplyWithOtherChanges &&
                                                                (d.AppliesTo.HasFlag(SyncOperation.Updates) || d.AppliesTo.HasFlag(SyncOperation.All)) &&
                                                                d.SyncSide == SyncSide.Target);

            var targetSideCustomSetFieldsForUpdateWithChangesOnly = map.CustomSetFields
                                                                .Where(d => d.OnlyApplyWithOtherChanges == true &&
                                                                            (d.AppliesTo.HasFlag(SyncOperation.Updates) || d.AppliesTo.HasFlag(SyncOperation.All)) &&
                                                                d.SyncSide == SyncSide.Target);

            Dictionary<string, DataOnlyField> targetSideDataOnlyFields = null;
            List<CompareField> targetSideCompareFieldsForDataOnlyFields = null;

            if (targetEntityBatch != null)
            {
                targetSideDataOnlyFields = targetEntityBatch.EntityDefinition.DataOnlyFields.ToDictionary(d => d.FieldName, d => d, StringComparer.OrdinalIgnoreCase);

                targetSideCompareFieldsForDataOnlyFields = map.CompareFields.Where(d => targetSideDataOnlyFields.Keys.Contains(d.TargetFieldToUpdate, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            ConflictResolutionResult winningSide;
            SyncSide syncSideToUpdate;
            SyncSide syncSideWithoutUpdate;
            IEnumerable<CustomSetField> customSetFieldsForUpdate;
            IEnumerable<CustomSetField> customSetFieldsForUpdateWithChangesOnly;
            IEnumerable<CustomSetField> customSetFieldsWithoutUpdates;
            Dictionary<string, DataOnlyField> dataOnlyFieldsForUpdate;
            Dictionary<string, DataOnlyField> dataOnlyFieldsWithoutUpdates;
            EntityBatch entityBatchToUpdate;
            EntityBatch entityBatchWithoutUpdate;
            IEnumerable<CompareField> compareFieldsForDataOnlyFieldsWithoutUpdate;

            foreach (DataRow row in rowsToCompare)
            {
                winningSide = map.ConflictResolutionRule(row);

                UpdateWinningSideInRow(row, winningSide);

                if (winningSide == ConflictResolutionResult.TargetWon &&
                    sourceEntityBatch.EntityDefinition.UpdatesToExcludeFilter != null &&
                    sourceEntityBatch.EntityDefinition.UpdatesToExcludeFilter(row))
                {
                    continue;
                }

                if (winningSide == ConflictResolutionResult.SourceWon &&
                    targetEntityBatch.EntityDefinition.UpdatesToExcludeFilter != null &&
                    targetEntityBatch.EntityDefinition.UpdatesToExcludeFilter(row))
                {
                    continue;
                }

                if (winningSide == ConflictResolutionResult.SourceWon)
                {
                    syncSideToUpdate = SyncSide.Target;
                    syncSideWithoutUpdate = SyncSide.Source;
                    dataOnlyFieldsForUpdate = targetSideDataOnlyFields;
                    dataOnlyFieldsWithoutUpdates = sourceSideDataOnlyFields;
                    customSetFieldsForUpdate = targetSideCustomSetFieldsForUpdate;
                    customSetFieldsForUpdateWithChangesOnly = targetSideCustomSetFieldsForUpdateWithChangesOnly;
                    customSetFieldsWithoutUpdates = sourceSideCustomSetFieldsForUpdate;
                    entityBatchToUpdate = targetEntityBatch;
                    entityBatchWithoutUpdate = sourceEntityBatch;
                    compareFieldsForDataOnlyFieldsWithoutUpdate = sourceSideCompareFieldsForDataOnlyFields;
                }
                else if (winningSide == ConflictResolutionResult.TargetWon)
                {
                    syncSideToUpdate = SyncSide.Source;
                    syncSideWithoutUpdate = SyncSide.Target;
                    dataOnlyFieldsForUpdate = sourceSideDataOnlyFields;
                    dataOnlyFieldsWithoutUpdates = targetSideDataOnlyFields;
                    customSetFieldsForUpdate = sourceSideCustomSetFieldsForUpdate;
                    customSetFieldsForUpdateWithChangesOnly = sourceSideCustomSetFieldsForUpdateWithChangesOnly;
                    customSetFieldsWithoutUpdates = targetSideCustomSetFieldsForUpdate;
                    entityBatchToUpdate = sourceEntityBatch;
                    entityBatchWithoutUpdate = targetEntityBatch;
                    compareFieldsForDataOnlyFieldsWithoutUpdate = targetSideCompareFieldsForDataOnlyFields;
                }
                else
                    throw new EnumValueNotImplementedException<ConflictResolutionResult>(winningSide);

                // only proceed with comparisons if the non-winning side should have updates applied
                if ((winningSide == ConflictResolutionResult.SourceWon && applyUpdatesToTarget) ||
                    (winningSide == ConflictResolutionResult.TargetWon && applyUpdatesToSource))
                {
                    ProcessSideWithUpdates(map, entityBatchToUpdate, row, winningSide, syncSideToUpdate, dataOnlyFieldsForUpdate, customSetFieldsForUpdate, customSetFieldsForUpdateWithChangesOnly);
                }
                
                if (map is TwoWayDataMap)
                {
                    // add primary keys, secondary keys, and data only fields for losing side
                    ProcessSideWithoutUpdates(map, compareFieldsForDataOnlyFieldsWithoutUpdate, entityBatchWithoutUpdate, row, winningSide, syncSideWithoutUpdate, dataOnlyFieldsWithoutUpdates, customSetFieldsWithoutUpdates);

                    // add primary keys, secondary keys, and data only fields for winning side w/o updates enabled
                    if ((winningSide == ConflictResolutionResult.SourceWon && !applyUpdatesToTarget) ||
                        (winningSide == ConflictResolutionResult.TargetWon && !applyUpdatesToSource))
                    {
                        if (winningSide == ConflictResolutionResult.SourceWon)
                        {
                            winningSide = ConflictResolutionResult.TargetWon;
                        }
                        else if (winningSide == ConflictResolutionResult.TargetWon)
                        {
                            winningSide = ConflictResolutionResult.SourceWon;
                        }
                        else
                            throw new EnumValueNotImplementedException<ConflictResolutionResult>(winningSide);

                        ProcessSideWithoutUpdates(map, map.CompareFields, entityBatchToUpdate, row, winningSide, syncSideToUpdate, dataOnlyFieldsForUpdate, customSetFieldsForUpdate);
                    }
                }
            }
        }

        private static void ProcessSideWithUpdates(OneToOneDataMap map, EntityBatch entityBatchToUpdate, DataRow row, ConflictResolutionResult winningSide, SyncSide syncSideToUpdate, Dictionary<string, DataOnlyField> dataOnlyFields, IEnumerable<CustomSetField> customSetFieldsForUpdate, IEnumerable<CustomSetField> customSetFieldsForUpdateWithChangesOnly)
        {
            var primaryKeyValues = GetPrimaryKeyValuesFromData(map, row, syncSideToUpdate);

            var recordToUpdate = new RecordToUpdate(entityBatchToUpdate, primaryKeyValues);

            foreach (var compareField in map.CompareFields)
            {
                var sourceValue = row[compareField.SourceFieldToCompareWithPrefix];
                var targetValue = row[compareField.TargetFieldToCompareWithPrefix];

                string sourceValueAsString;

                if (sourceValue == DBNull.Value || sourceValue == null)
                    sourceValueAsString = null;
                else
                    sourceValueAsString = sourceValue.ToString();

                string targetValueAsString;

                if (targetValue == DBNull.Value || targetValue == null)
                    targetValueAsString = null;
                else
                    targetValueAsString = targetValue.ToString();

                UpdateValueSet oldAndNewValues;

                switch (compareField.Type)
                {
                    case CompareType.ConvertToType:

                        if (syncSideToUpdate == SyncSide.Target)
                            oldAndNewValues = new UpdateValueSet(targetValueAsString, sourceValueAsString);
                        else
                            oldAndNewValues = new UpdateValueSet(sourceValueAsString, targetValueAsString);

                        var typeFieldIsEqual = SyncFieldsAreEqual(row, compareField, compareField.CompareAs);

                        if (!typeFieldIsEqual
                            || (syncSideToUpdate == SyncSide.Target &&
                                dataOnlyFields.ContainsKey(compareField.TargetFieldToUpdate) &&
                                dataOnlyFields[compareField.TargetFieldToUpdate].MethodToPopulateValue == null)
                            || (syncSideToUpdate == SyncSide.Source &&
                                dataOnlyFields.ContainsKey(compareField.SourceFieldToUpdate) &&
                                dataOnlyFields[compareField.SourceFieldToUpdate].MethodToPopulateValue == null))
                        {
                            ApplyPreSaveConversion(syncSideToUpdate, row, compareField, ref oldAndNewValues.NewValue);
                        }

                        if (!typeFieldIsEqual)
                        {
                            if (syncSideToUpdate == SyncSide.Target)
                                recordToUpdate.AddFieldValuePair(compareField.TargetFieldToUpdate, oldAndNewValues);
                            else
                                recordToUpdate.AddFieldValuePair(compareField.SourceFieldToUpdate, oldAndNewValues);
                        }
                        break;

                    case CompareType.FieldMap:

                        string newValue = null;
                        bool fieldMapIsEqual = false;

                        fieldMapIsEqual = SyncFieldsAreEqual(compareField, row, compareField.FieldMap, syncSideToUpdate, ref newValue);

                        if (syncSideToUpdate == SyncSide.Target)
                            oldAndNewValues = new UpdateValueSet(targetValueAsString, newValue);
                        else
                            oldAndNewValues = new UpdateValueSet(sourceValueAsString, newValue);

                        if (!fieldMapIsEqual)
                        {
                            if (syncSideToUpdate == SyncSide.Target)
                                recordToUpdate.AddFieldValuePair(compareField.TargetFieldToUpdate, oldAndNewValues);
                            else
                                recordToUpdate.AddFieldValuePair(compareField.SourceFieldToUpdate, oldAndNewValues);
                        }
                        break;

                    case CompareType.CustomMethod:

                        if (syncSideToUpdate == SyncSide.Target)
                            oldAndNewValues = new UpdateValueSet(targetValueAsString, sourceValueAsString);
                        else
                            oldAndNewValues = new UpdateValueSet(sourceValueAsString, targetValueAsString);

                        var customConvertFieldIsEqual = SyncFieldsAreEqual(compareField, row, compareField.CustomCompareMethod);

                        if (!customConvertFieldIsEqual
                            || (syncSideToUpdate == SyncSide.Target &&
                                dataOnlyFields.ContainsKey(compareField.TargetFieldToUpdate) &&
                                dataOnlyFields[compareField.TargetFieldToUpdate].MethodToPopulateValue == null)
                            || (syncSideToUpdate == SyncSide.Source &&
                                dataOnlyFields.ContainsKey(compareField.SourceFieldToCompare) &&
                                dataOnlyFields[compareField.SourceFieldToCompare].MethodToPopulateValue == null))
                        {
                            ApplyPreSaveConversion(syncSideToUpdate, row, compareField, ref oldAndNewValues.NewValue);
                        }

                        if (!customConvertFieldIsEqual)
                        {
                            if (syncSideToUpdate == SyncSide.Target)
                                recordToUpdate.AddFieldValuePair(compareField.TargetFieldToUpdate, oldAndNewValues);
                            else
                                recordToUpdate.AddFieldValuePair(compareField.SourceFieldToUpdate, oldAndNewValues);
                        }
                        break;

                    default:
                        throw new EnumValueNotImplementedException<CompareType>(compareField.Type);
                }

                // add as data only field, if applicable
                if (syncSideToUpdate == SyncSide.Target &&
                    dataOnlyFields.ContainsKey(compareField.TargetFieldToUpdate) &&
                    dataOnlyFields[compareField.TargetFieldToUpdate].MethodToPopulateValue == null)
                {
                    recordToUpdate.DataOnlyValues.Add(compareField.TargetFieldToUpdate, oldAndNewValues.NewValue);
                }
                else if (syncSideToUpdate == SyncSide.Source &&
                         dataOnlyFields.ContainsKey(compareField.SourceFieldToUpdate) &&
                         dataOnlyFields[compareField.SourceFieldToUpdate].MethodToPopulateValue == null)
                {
                    recordToUpdate.DataOnlyValues.Add(compareField.SourceFieldToUpdate, oldAndNewValues.NewValue);
                }
            }

            // add data only fields with a custom Func to populate the value
            foreach (var dataOnlyFieldWithFunc in dataOnlyFields.Where(d => d.Value.MethodToPopulateValue != null))
            {
                recordToUpdate.DataOnlyValues.Add(dataOnlyFieldWithFunc.Key, (dataOnlyFieldWithFunc.Value.MethodToPopulateValue(row) ?? "").ToString());
            }

            foreach (var customSetField in customSetFieldsForUpdate)
            {
                object oldValue = row[customSetField.FieldNameToCompareWithPrefix] == DBNull.Value ? null : row[customSetField.FieldNameToCompareWithPrefix];

                string oldValueAsString = oldValue == null ? null : oldValue.ToString();

                var customSetValue = customSetField.CustomSetMethod(row);

                var oldAndNewValues = new UpdateValueSet(oldValueAsString, customSetValue == null ? null : customSetValue.ToString());

                if (!FieldComparer.AreEqualOfUnknownType(oldValue, customSetValue))
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

            foreach (var customSetField in customSetFieldsForUpdateWithChangesOnly)
            {
                object oldValue = row[customSetField.FieldNameToCompareWithPrefix] == DBNull.Value ? null : row[customSetField.FieldNameToCompareWithPrefix];

                string oldValueAsString = oldValue == null ? null : oldValue.ToString();

                var customSetValue = customSetField.CustomSetMethod(row);

                var oldAndNewValues = new UpdateValueSet(oldValueAsString, customSetValue == null ? null : customSetValue.ToString());

                if (recordToUpdate.FieldValuePairs.Count > 0 && !FieldComparer.AreEqualOfUnknownType(oldValue, customSetValue))
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

            // add secondary keys, if applicable
            AddSecondaryKeyValues(map, winningSide, recordToUpdate);

            // add record to batch
            if (recordToUpdate.FieldValuePairs.Count > 0)
                entityBatchToUpdate.RecordsToUpdate.Add(recordToUpdate);
            else if (recordToUpdate.DataOnlyValues.Count > 0)
            {
                var recordWithoutChange = new EntityRecordWithoutChange(entityBatchToUpdate, recordToUpdate.PrimaryKeyValues.ToList());

                recordWithoutChange.DataOnlyValues = recordToUpdate.DataOnlyValues;

                recordWithoutChange.SecondaryKeyValues = recordToUpdate.SecondaryKeyValues;

                entityBatchToUpdate.RecordsWithoutChange.Add(recordWithoutChange);
            }
        }

        private static void ProcessSideWithoutUpdates(OneToOneDataMap map, IEnumerable<CompareField> dataOnlyCompareFields, EntityBatch entityBatchWithoutUpdate, DataRow row, ConflictResolutionResult winningSide, SyncSide syncSideWithoutUpdate, Dictionary<string, DataOnlyField> dataOnlyFields, IEnumerable<CustomSetField> customSetFieldsForUpdate)
        {
            var primaryKeyValues = GetPrimaryKeyValuesFromData(map, row, syncSideWithoutUpdate);

            var recordWithoutChange = new EntityRecordWithoutChange(entityBatchWithoutUpdate, primaryKeyValues);

            foreach (var compareField in dataOnlyCompareFields)
            {
                var sourceValue = row[compareField.SourceFieldToCompareWithPrefix];
                var targetValue = row[compareField.TargetFieldToCompareWithPrefix];

                string sourceValueAsString;

                if (sourceValue == DBNull.Value || sourceValue == null)
                    sourceValueAsString = null;
                else
                    sourceValueAsString = sourceValue.ToString();

                string targetValueAsString;

                if (targetValue == DBNull.Value || targetValue == null)
                    targetValueAsString = null;
                else
                    targetValueAsString = targetValue.ToString();

                UpdateValueSet oldAndNewValues;

                switch (compareField.Type)
                {
                    case CompareType.ConvertToType:

                        if (syncSideWithoutUpdate == SyncSide.Target)
                            oldAndNewValues = new UpdateValueSet(targetValueAsString, sourceValueAsString);
                        else
                            oldAndNewValues = new UpdateValueSet(sourceValueAsString, targetValueAsString);

                        var typeFieldIsEqual = SyncFieldsAreEqual(row, compareField, compareField.CompareAs);

                        if (!typeFieldIsEqual
                            || (syncSideWithoutUpdate == SyncSide.Target &&
                                dataOnlyFields.ContainsKey(compareField.TargetFieldToUpdate) &&
                                dataOnlyFields[compareField.TargetFieldToUpdate].MethodToPopulateValue == null)
                            || (syncSideWithoutUpdate == SyncSide.Source &&
                                dataOnlyFields.ContainsKey(compareField.SourceFieldToUpdate) &&
                                dataOnlyFields[compareField.SourceFieldToUpdate].MethodToPopulateValue == null))
                        {
                            ApplyPreSaveConversion(syncSideWithoutUpdate, row, compareField, ref oldAndNewValues.NewValue);
                        }

                        if (!typeFieldIsEqual)
                        {
                            if (syncSideWithoutUpdate == SyncSide.Target)
                                recordWithoutChange.DataOnlyValues.Add(compareField.TargetFieldToUpdate, oldAndNewValues.NewValue);
                            else
                                recordWithoutChange.DataOnlyValues.Add(compareField.SourceFieldToUpdate, oldAndNewValues.NewValue);
                        }
                        break;

                    case CompareType.FieldMap:

                        string newValue = null;
                        bool fieldMapIsEqual = false;

                        fieldMapIsEqual = SyncFieldsAreEqual(compareField, row, compareField.FieldMap, syncSideWithoutUpdate, ref newValue);

                        if (syncSideWithoutUpdate == SyncSide.Target)
                            oldAndNewValues = new UpdateValueSet(targetValueAsString, newValue);
                        else
                            oldAndNewValues = new UpdateValueSet(sourceValueAsString, newValue);

                        if (!fieldMapIsEqual)
                        {
                            if (syncSideWithoutUpdate == SyncSide.Target)
                                recordWithoutChange.DataOnlyValues.Add(compareField.TargetFieldToUpdate, oldAndNewValues.NewValue);
                            else
                                recordWithoutChange.DataOnlyValues.Add(compareField.SourceFieldToUpdate, oldAndNewValues.NewValue);
                        }
                        break;

                    case CompareType.CustomMethod:

                        if (syncSideWithoutUpdate == SyncSide.Target)
                            oldAndNewValues = new UpdateValueSet(targetValueAsString, sourceValueAsString);
                        else
                            oldAndNewValues = new UpdateValueSet(sourceValueAsString, targetValueAsString);

                        var customConvertFieldIsEqual = SyncFieldsAreEqual(compareField, row, compareField.CustomCompareMethod);

                        if (!customConvertFieldIsEqual
                            || (syncSideWithoutUpdate == SyncSide.Target &&
                                dataOnlyFields.ContainsKey(compareField.TargetFieldToUpdate) &&
                                dataOnlyFields[compareField.TargetFieldToUpdate].MethodToPopulateValue == null)
                            || (syncSideWithoutUpdate == SyncSide.Source &&
                                dataOnlyFields.ContainsKey(compareField.SourceFieldToCompare) &&
                                dataOnlyFields[compareField.SourceFieldToCompare].MethodToPopulateValue == null))
                        {
                            ApplyPreSaveConversion(syncSideWithoutUpdate, row, compareField, ref oldAndNewValues.NewValue);
                        }

                        if (!customConvertFieldIsEqual)
                        {
                            if (syncSideWithoutUpdate == SyncSide.Target)
                                recordWithoutChange.DataOnlyValues.Add(compareField.TargetFieldToUpdate, oldAndNewValues.NewValue);
                            else
                                recordWithoutChange.DataOnlyValues.Add(compareField.SourceFieldToUpdate, oldAndNewValues.NewValue);
                        }
                        break;

                    default:
                        throw new EnumValueNotImplementedException<CompareType>(compareField.Type);
                }

                // add as data only field, if applicable
                if (syncSideWithoutUpdate == SyncSide.Target &&
                    dataOnlyFields.ContainsKey(compareField.TargetFieldToUpdate) &&
                    dataOnlyFields[compareField.TargetFieldToUpdate].MethodToPopulateValue == null)
                {
                    recordWithoutChange.DataOnlyValues.Add(compareField.TargetFieldToUpdate, oldAndNewValues.NewValue);
                }
                else if (syncSideWithoutUpdate == SyncSide.Source &&
                         dataOnlyFields.ContainsKey(compareField.SourceFieldToUpdate) &&
                         dataOnlyFields[compareField.SourceFieldToUpdate].MethodToPopulateValue == null)
                {
                    recordWithoutChange.DataOnlyValues.Add(compareField.SourceFieldToUpdate, oldAndNewValues.NewValue);
                }
            }

            // add data only fields with a custom Func to populate the value
            foreach (var dataOnlyFieldWithFunc in dataOnlyFields.Where(d => d.Value.MethodToPopulateValue != null))
            {
                recordWithoutChange.DataOnlyValues.Add(dataOnlyFieldWithFunc.Key, (dataOnlyFieldWithFunc.Value.MethodToPopulateValue(row) ?? "").ToString());
            }

            foreach (var customSetField in customSetFieldsForUpdate)
            {
                // add as data only field, if applicable
                if (dataOnlyFields.ContainsKey(customSetField.FieldNameToUpdate) &&
                    dataOnlyFields[customSetField.FieldNameToUpdate].MethodToPopulateValue == null)
                {
                    object oldValue = row[customSetField.FieldNameToCompareWithPrefix] == DBNull.Value ? null : row[customSetField.FieldNameToCompareWithPrefix];

                    string oldValueAsString = oldValue == null ? null : oldValue.ToString();

                    var customSetValue = customSetField.CustomSetMethod(row);

                    var oldAndNewValues = new UpdateValueSet(oldValueAsString, customSetValue == null ? null : customSetValue.ToString());
                
                    recordWithoutChange.DataOnlyValues.Add(customSetField.FieldNameToUpdate, oldAndNewValues.NewValue);
                }
            }

            // add secondary keys, if applicable
            AddSecondaryKeyValues(map, winningSide == ConflictResolutionResult.SourceWon ? ConflictResolutionResult.TargetWon : ConflictResolutionResult.SourceWon, recordWithoutChange);

            // add record to batch
            entityBatchWithoutUpdate.RecordsWithoutChange.Add(recordWithoutChange);
        }

        private static void UpdateWinningSideInRow(DataRow row, ConflictResolutionResult winningSide)
        {
            if (winningSide == ConflictResolutionResult.SourceWon)
                row["WinningSide"] = "Source";
            else if (winningSide == ConflictResolutionResult.TargetWon)
                row["WinningSide"] = "Target";
            else
                throw new EnumValueNotImplementedException<ConflictResolutionResult>(winningSide);
        }

        private static List<string> GetPrimaryKeyValuesFromData(OneToOneDataMap map, DataRow row, SyncSide syncSide)
        {
            List<string> primaryKeyValues = new List<string>();

            if (map is OneWayDataMap)
            {
                foreach (var primaryKeyColumnNameWithPrefix in ((OneWayDataMap)map).EntityToUpdateDefinition.PrimaryKeyColumnNamesWithPrefix)
                {
                    primaryKeyValues.Add(row[primaryKeyColumnNameWithPrefix].ToString());
                }
            }
            else if (map is TwoWayDataMap)
            {
                if (syncSide == SyncSide.Source)
                {
                    foreach (var primaryKeyColumnNameWithPrefix in ((TwoWayDataMap)map).SourceDefinition.PrimaryKeyColumnNamesWithPrefix)
                    {
                        primaryKeyValues.Add(row[primaryKeyColumnNameWithPrefix].ToString());
                    }
                }
                else if (syncSide == SyncSide.Target)
                {
                    foreach (var primaryKeyColumnNameWithPrefix in ((TwoWayDataMap)map).TargetDefinition.PrimaryKeyColumnNamesWithPrefix)
                    {
                        primaryKeyValues.Add(row[primaryKeyColumnNameWithPrefix].ToString());
                    }
                }
                else
                    throw new EnumValueNotImplementedException<SyncSide>(syncSide);
            }
            else
                throw new DerivedClassNotImplementedException<OneToOneDataMap>(map);

            return primaryKeyValues;
        }

        private static void AddSecondaryKeyValues(OneToOneDataMap map, ConflictResolutionResult winningSide, EntityRecord record)
        {
            BindingList<string> secondaryKeyColumnNames = null;

            if (map is OneWayDataMap)
            {
                var oneWayMap = (OneWayDataMap)map;

                secondaryKeyColumnNames = oneWayMap.EntityToUpdateDefinition.SecondaryKeyColumnNames;
            }
            else if (map is TwoWayDataMap)
            {
                var twoWayMap = (TwoWayDataMap)map;

                if (winningSide == ConflictResolutionResult.SourceWon)
                    secondaryKeyColumnNames = twoWayMap.TargetDefinition.SecondaryKeyColumnNames;
                else if (winningSide == ConflictResolutionResult.TargetWon)
                    secondaryKeyColumnNames = twoWayMap.SourceDefinition.SecondaryKeyColumnNames;
                else
                    throw new EnumValueNotImplementedException<ConflictResolutionResult>(winningSide);
            }
            else
                throw new DerivedClassNotImplementedException<OneToOneDataMap>(map);

            if (secondaryKeyColumnNames != null && secondaryKeyColumnNames.Count > 0)
            {
                record.SecondaryKeyValues = new List<string>();

                foreach (var secondaryKeyColumnName in secondaryKeyColumnNames)
                {
                    record.SecondaryKeyValues.Add(record.DataOnlyValues[secondaryKeyColumnName]);
                }
            }
        }

        public static void ApplyPreSaveConversion(SyncSide syncSide, DataRow row, CompareField compareField, ref string fromValue)
        {
            switch (syncSide)
            {
                case SyncSide.Target:

                    if (compareField.TargetPreSaveConversionMethod != null)
                    {
                        var convertedValue = compareField.TargetPreSaveConversionMethod(row, fromValue);

                        if (convertedValue == null)
                            fromValue = null;
                        else
                            fromValue = convertedValue.ToString();
                    }

                    break;

                case SyncSide.Source:

                    if (compareField.SourcePreSaveConversionMethod != null)
                    {
                        var convertedValue = compareField.SourcePreSaveConversionMethod(row, fromValue);

                        if (convertedValue == null)
                            fromValue = null;
                        else
                            fromValue = convertedValue.ToString();
                    }

                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncSide>(syncSide);
            }
        }

        private static bool SyncFieldsAreEqual(DataRow row, CompareField syncField, CompareAs compareAsType)
        {
            var sourceValue = row[syncField.SourceFieldToCompareWithPrefix];

            var targetValue = row[syncField.TargetFieldToCompareWithPrefix];

            switch(compareAsType)
            {
                case CompareAs.Boolean:
                    return FieldComparer.AreEqualBooleans(sourceValue, targetValue);

                case CompareAs.DateTime:
                    return FieldComparer.AreEqualDateTimes(sourceValue, targetValue);

                case CompareAs.DateOnly:
                    return FieldComparer.AreEqualDateTimes(sourceValue, targetValue, true);

                case CompareAs.Integer:
                    return FieldComparer.AreEqualIntegers(sourceValue, targetValue);

                case CompareAs.Numeric:
                    return FieldComparer.AreEqualNumerics(sourceValue, targetValue);

                case CompareAs.String:
                    return FieldComparer.AreEqualStrings(sourceValue, targetValue);

                case CompareAs.String_IgnoreCase:
                    return FieldComparer.AreEqualStrings(sourceValue, targetValue, true);

                default:
                    throw new EnumValueNotImplementedException<CompareAs>(compareAsType);
            }
        }

        private static bool SyncFieldsAreEqual(CompareField syncField, DataRow row, FieldMap fieldMap, SyncSide syncSideToUpdate, ref string newValue)
        {
            var sourceValue = row[syncField.SourceFieldToCompareWithPrefix] == DBNull.Value ? null : row[syncField.SourceFieldToCompareWithPrefix].ToString();
            var targetValue = row[syncField.TargetFieldToCompareWithPrefix] == DBNull.Value ? null : row[syncField.TargetFieldToCompareWithPrefix].ToString();

            if (syncSideToUpdate == SyncSide.Source)
                return FieldComparer.AreEqual(syncField.SourceFieldToUpdate, sourceValue, targetValue, fieldMap, syncSideToUpdate, ref newValue);
            else if (syncSideToUpdate == SyncSide.Target)
                return FieldComparer.AreEqual(syncField.TargetFieldToUpdate, sourceValue, targetValue, fieldMap, syncSideToUpdate, ref newValue);
            else
                throw new EnumValueNotImplementedException<SyncSide>(syncSideToUpdate);
        }

        private static bool SyncFieldsAreEqual(CompareField syncField, DataRow row, Func<DataRow, object, object, ComparisonResult> customMethod)
        {
            var sourceValue = row[syncField.SourceFieldToCompareWithPrefix] == DBNull.Value ? null : row[syncField.SourceFieldToCompareWithPrefix];
            var oldTargetValue = row[syncField.TargetFieldToCompareWithPrefix] == DBNull.Value ? null : row[syncField.TargetFieldToCompareWithPrefix];

            return FieldComparer.AreEqual(row, sourceValue, oldTargetValue, customMethod);
        }

        private static void AddDeletionsToBatch(ref EntityBatch batch, DataTable combinedTable, OneWayDataMap map)
        {
            string joinFieldToCheckForNull;
            
            switch (map.SyncDirection)
	        {
		        case SyncDirection.SourceToTarget:
                    joinFieldToCheckForNull = map.JoinKeysCollection.JoinFields[0].SourceJoinFieldWithPrefix;
                    break;

                case SyncDirection.TargetToSource:
                    joinFieldToCheckForNull = map.JoinKeysCollection.JoinFields[0].TargetJoinFieldWithPrefix;
                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncDirection>(map.SyncDirection);
	        }

            var rowsToDelete = from row in combinedTable.AsEnumerable()
                               where row.Field<object>(joinFieldToCheckForNull) == null
                               select row;

            var customSetFieldsForDelete = map.CustomSetFields.Where(d => d.AppliesTo.HasFlag(SyncOperation.Deletes) || d.AppliesTo.HasFlag(SyncOperation.All));

            var dataOnlyFields = batch.EntityDefinition.DataOnlyFields.ToDictionary(d => d.FieldName, d => d, StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in rowsToDelete)
            {
                UpdateWinningSideInRow(row, batch.EntityDefinition.SyncSide == SyncSide.Source ? ConflictResolutionResult.TargetWon : ConflictResolutionResult.SourceWon);

                if (batch.EntityDefinition.DeletionsToExcludeFilter != null &&
                    batch.EntityDefinition.DeletionsToExcludeFilter(row))
                {
                    continue;
                }

                List<string> primaryKeyValues = new List<string>();

                foreach (var primaryKeyColumnNameWithPrefix in map.EntityToUpdateDefinition.PrimaryKeyColumnNamesWithPrefix)
                    primaryKeyValues.Add(row[primaryKeyColumnNameWithPrefix].ToString());

                var recordToDelete = new RecordToDelete(batch, primaryKeyValues);

                foreach (var customSetField in customSetFieldsForDelete)
                {
                    object oldValue = row[customSetField.FieldNameToCompareWithPrefix] == DBNull.Value ? null : row[customSetField.FieldNameToCompareWithPrefix];

                    string oldValueAsString = oldValue == null ? null : oldValue.ToString();

                    var customSetValue = customSetField.CustomSetMethod(row);

                    if (!FieldComparer.AreEqualOfUnknownType(oldValue, customSetValue))
                    {
                        var oldAndNewValues = new UpdateValueSet(oldValueAsString, customSetValue == null ? null : customSetValue.ToString());

                        recordToDelete.AddFieldValuePair(customSetField.FieldNameToUpdate, oldAndNewValues);
                    }
                }

                foreach (var dataOnlyFieldWithFunc in dataOnlyFields.Where(d => d.Value.MethodToPopulateValue != null))
                {
                    recordToDelete.DataOnlyValues.Add(dataOnlyFieldWithFunc.Key, (dataOnlyFieldWithFunc.Value.MethodToPopulateValue(row) ?? "").ToString());
                }

                // add secondary keys, if applicable
                var winningSide = map.EntityToUpdateDefinition.SyncSide == SyncSide.Source ? ConflictResolutionResult.TargetWon : ConflictResolutionResult.SourceWon;
                AddSecondaryKeyValues(map, winningSide, recordToDelete);

                batch.RecordsToDelete.Add(recordToDelete);
            }
        }

        /// <summary>
        /// Apply the data map to generate batch exclusively of RecordToAdd instances for all data rows.
        /// </summary>
        /// <returns></returns>
        public static EntityBatch GetInsertsForAll(OneWayDataMap map, DataTable data, bool dataColumnNamesIncludeSyncSidePrefix = false)
        {
            if (map == null)
                throw new Exception("Data map can not be null.");

            if (data == null)
                throw new Exception("Data table can not be null.");

            if (!map.EntityToUpdateDefinition.ApplyInserts)
                return null;

            ValidatateEntityToUpdateDefinition(map);

            if (!data.Columns.Contains("WinningSide"))
                data.Columns.Add("WinningSide", typeof(string));

            if (map.EntityToUpdateDefinition.SyncSide == SyncSide.Source)
                ValidateDataFieldsExistInDataTable(SyncSide.Target, map, data, dataColumnNamesIncludeSyncSidePrefix);
            else if (map.EntityToUpdateDefinition.SyncSide == SyncSide.Target)
                ValidateDataFieldsExistInDataTable(SyncSide.Source, map, data, dataColumnNamesIncludeSyncSidePrefix);
            else
                throw new EnumValueNotImplementedException<SyncSide>(map.EntityToUpdateDefinition.SyncSide);

            ValidateDataOnlyFieldsExistInMap(map);

            var entityBatch = new EntityBatch(map.EntityToUpdateDefinition);

            AddInsertsToBatch(ref entityBatch, map, map.EntityToUpdateDefinition.SyncSide, data.Rows.Cast<DataRow>(), dataColumnNamesIncludeSyncSidePrefix);

            return entityBatch;
        }

        /// <summary>
        /// Apply the data map on the specified sync side to generate batch exclusively of RecordToAdd instances for all data rows.
        /// </summary>
        /// <returns></returns>
        public static EntityBatch GetInsertsForAll(SyncSide syncSideToUpdate, TwoWayDataMap map, DataTable data, bool dataColumnNamesIncludeSyncSidePrefix = false)
        {
            if (map == null)
                throw new Exception("Data map can not be null.");

            if (data == null)
                throw new Exception("Data table can not be null.");

            ValidatateEntityToUpdateDefinition(map);

            if (!data.Columns.Contains("WinningSide"))
                data.Columns.Add("WinningSide", typeof(string));

            EntityToUpdateDefinition entityDefinition;

            if (syncSideToUpdate == SyncSide.Source)
            {
                if (!map.SourceDefinition.ApplyInserts)
                    return null;

                entityDefinition = map.SourceDefinition;
            }
            else if (syncSideToUpdate == SyncSide.Target)
            {
                if (!map.TargetDefinition.ApplyInserts)
                    return null;

                entityDefinition = map.TargetDefinition;
            }
            else
                throw new EnumValueNotImplementedException<SyncSide>(syncSideToUpdate);

            if (syncSideToUpdate == SyncSide.Source)
                ValidateDataFieldsExistInDataTable(SyncSide.Target, map, data, dataColumnNamesIncludeSyncSidePrefix);
            else if (syncSideToUpdate == SyncSide.Target)
                ValidateDataFieldsExistInDataTable(SyncSide.Source, map, data, dataColumnNamesIncludeSyncSidePrefix);
            else
                throw new EnumValueNotImplementedException<SyncSide>(syncSideToUpdate);

            ValidateDataOnlyFieldsExistInMap(map);

            var entityBatch = new EntityBatch(entityDefinition);

            AddInsertsToBatch(ref entityBatch, map, syncSideToUpdate, data.Rows.Cast<DataRow>(), dataColumnNamesIncludeSyncSidePrefix);

            return entityBatch;
        }
    }
}