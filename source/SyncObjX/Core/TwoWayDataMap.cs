using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SyncObjX.Data;
using SyncObjX.Exceptions;

namespace SyncObjX.Core
{
    public class TwoWayDataMap : OneToOneDataMap
    {
        public readonly EntityToUpdateDefinition SourceDefinition;

        public readonly EntityToUpdateDefinition TargetDefinition;

        public override DataTransferType TransferType
        {
            get { return DataTransferType.BiDirectional; }
        }

        public TwoWayDataMap(EntityToUpdateDefinition sourceDefinition, EntityToUpdateDefinition targetDefinition, 
                                    JoinFieldCollection joinKeys, Func<DataRow, ConflictResolutionResult> conflictResolutionRule)
            : base(joinKeys, conflictResolutionRule) 
        {
            if (sourceDefinition == null)
                throw new Exception("Source entity definition can not be null.");

            if (sourceDefinition.SyncSide != SyncSide.Source)
                throw new Exception("Source entity definition can not be configured as target-side.");

            if (targetDefinition == null)
                throw new Exception("Target entity definition can not be null.");

            if (targetDefinition.SyncSide != SyncSide.Target)
                throw new Exception("Target entity definition can not be configured as source-side.");

            SourceDefinition = sourceDefinition;

            TargetDefinition = targetDefinition;
        }

        public new void AddCompareField(string sourceField, string targetField, CompareAs compareAsType)
        {
            AddCompareField(sourceField, sourceField, null, targetField, targetField, null, compareAsType);
        }

        public new void AddCompareField(string sourceField, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                                        string targetField, Func<DataRow, object, object> targetPreSaveConversionMethod,
                                        CompareAs compareAsType)
        {
            base.AddCompareField(sourceField, sourceField, sourcePreSaveConversionMethod,
                                 targetField, targetField, targetPreSaveConversionMethod,
                                 compareAsType);
        }

        public void AddCompareField(string sourceFieldToCompare, string sourceFieldToUpdate, 
                                    string targetFieldToCompare, string targetFieldToUpdate, 
                                    CompareAs compareAsType)
        {
            AddCompareField(sourceFieldToCompare, sourceFieldToUpdate, null, targetFieldToCompare, targetFieldToUpdate, null, compareAsType);
        }

        public new void AddCompareField(string sourceFieldToCompare, string sourceFieldToUpdate, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                                        string targetFieldToCompare, string targetFieldToUpdate, Func<DataRow, object, object> targetPreSaveConversionMethod,
                                        CompareAs compareAsType)
        {
            base.AddCompareField(sourceFieldToCompare, sourceFieldToUpdate, sourcePreSaveConversionMethod,
                                 targetFieldToCompare, targetFieldToUpdate, targetPreSaveConversionMethod,
                                 compareAsType);
        }

        public void AddCompareField(string sourceField, string targetField, TwoWayFieldMap fieldMap)
        {
            base.AddCompareField(sourceField, targetField, fieldMap);
        }

        public void AddCompareField(string sourceFieldToCompare, string sourceFieldToUpdate,
                                    string targetFieldToCompare, string targetFieldToUpdate,
                                    TwoWayFieldMap fieldMap)
        {
            base.AddCompareField(sourceFieldToCompare, sourceFieldToUpdate, targetFieldToCompare, targetFieldToUpdate, fieldMap);
        }

        public new void AddCompareField(string sourceField, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                                        string targetField, Func<DataRow, object, object> targetPreSaveConversionMethod,
                                        Func<DataRow, object, object, ComparisonResult> customCompareMethod)
        {
            base.AddCompareField(sourceField, sourceField, sourcePreSaveConversionMethod, targetField, targetField, targetPreSaveConversionMethod, customCompareMethod);
        }

        public new void AddCompareField(string sourceFieldToCompare, string sourceFieldToUpdate, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                                        string targetFieldToCompare, string targetFieldToUpdate, Func<DataRow, object, object> targetPreSaveConversionMethod,
                                        Func<DataRow, object, object, ComparisonResult> customCompareMethod)
        {
            base.AddCompareField(sourceFieldToCompare, sourceFieldToUpdate, sourcePreSaveConversionMethod, 
                                 targetFieldToCompare, targetFieldToUpdate, targetPreSaveConversionMethod,
                                 customCompareMethod);
        }

        public void AddCustomSetField(SyncSide syncSide, string fieldToUpdate, Func<DataRow, object> customSetMethod, SyncOperation appliesTo, bool onlyApplyWithOtherChanges = false)
        {
            base.AddCustomSetField(syncSide, fieldToUpdate, fieldToUpdate, customSetMethod, appliesTo, onlyApplyWithOtherChanges);
        }

        public new void AddCustomSetField(SyncSide syncSide, string fieldToCompare, string fieldToUpdate, Func<DataRow, object> customSetMethod, SyncOperation appliesTo, bool onlyApplyWithOtherChanges = false)
        {
            base.AddCustomSetField(syncSide, fieldToCompare, fieldToUpdate, customSetMethod, appliesTo, onlyApplyWithOtherChanges);
        }

        public void AddAutoSetField(SyncSide syncSide, string fieldToUpdate, object value, SyncOperation appliesTo, bool onlyApplyWithOtherChanges = false)
        {
            AddAutoSetField(syncSide, fieldToUpdate, fieldToUpdate, value, appliesTo, onlyApplyWithOtherChanges);
        }

        public void AddAutoSetField(SyncSide syncSide, string fieldToCompare, string fieldToUpdate, object value, SyncOperation appliesTo, bool onlyApplyWithOtherChanges = false)
        {
            base.AddCustomSetField(syncSide, fieldToCompare, fieldToUpdate, (row) => { return value; }, appliesTo, onlyApplyWithOtherChanges);
        }

        public override HashSet<string> GetMappedDataFieldNames(SyncSide syncSide, DataMapFieldType includedFieldTypes = DataMapFieldType.All, string prefix = "")
        {
            HashSet<string> dataFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (syncSide == SyncSide.Source)
            {
                if (includedFieldTypes.HasFlag(DataMapFieldType.PrimaryKey) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                {
                    foreach (var primaryKey in SourceDefinition.PrimaryKeyColumnNames)
                    {
                        dataFields.Add(prefix + primaryKey);
                    }
                }

                if (includedFieldTypes.HasFlag(DataMapFieldType.SecondaryKey) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                {
                    foreach (var secondaryKey in SourceDefinition.SecondaryKeyColumnNames)
                    {
                        dataFields.Add(prefix + secondaryKey);
                    }
                }

                if (includedFieldTypes.HasFlag(DataMapFieldType.CompareField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                {
                    foreach (var compareField in CompareFields)
                    {
                        dataFields.Add(prefix + compareField.SourceFieldToCompare);
                    }
                }

                if (includedFieldTypes.HasFlag(DataMapFieldType.DataOnlyField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                {
                    foreach (var dataOnlyField in SourceDefinition.DataOnlyFields)
                    {
                        dataFields.Add(prefix + dataOnlyField.FieldName);
                    }
                }
            }
            else if (syncSide == SyncSide.Target)
            {
                if (includedFieldTypes.HasFlag(DataMapFieldType.PrimaryKey) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                {
                    foreach (var primaryKey in TargetDefinition.PrimaryKeyColumnNames)
                    {
                        dataFields.Add(prefix + primaryKey);
                    }
                }

                if (includedFieldTypes.HasFlag(DataMapFieldType.SecondaryKey) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                {
                    foreach (var secondaryKey in TargetDefinition.SecondaryKeyColumnNames)
                    {
                        dataFields.Add(prefix + secondaryKey);
                    }
                }

                if (includedFieldTypes.HasFlag(DataMapFieldType.CompareField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                {
                    foreach (var compareField in CompareFields)
                    {
                        dataFields.Add(prefix + compareField.TargetFieldToCompare);
                    }
                }

                if (includedFieldTypes.HasFlag(DataMapFieldType.DataOnlyField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                {
                    foreach (var dataOnlyField in TargetDefinition.DataOnlyFields)
                    {
                        dataFields.Add(prefix + dataOnlyField.FieldName);
                    }
                }
            }
            else
                throw new EnumValueNotImplementedException<SyncSide>(syncSide);

            if (includedFieldTypes.HasFlag(DataMapFieldType.CustomSetField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
            {
                foreach (var customSetField in CustomSetFields.Where(d => d.SyncSide == syncSide))
                {
                    dataFields.Add(prefix + customSetField.FieldNameToCompare);
                }
            }

            return dataFields;
        }
    }
}
