using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SyncObjX.Data;
using SyncObjX.Exceptions;

namespace SyncObjX.Core
{
    public class OneWayDataMap : OneToOneDataMap, IOneWayDataMap
    {
        public override DataTransferType TransferType
        {
            get { return DataTransferType.Unidirectional; }
        }

        public SyncDirection SyncDirection { get; private set; }

        public EntityToUpdateDefinition EntityToUpdateDefinition { get; private set; }

        public OneWayDataMap(SyncDirection syncDirection, JoinFieldCollection joinKeys, 
                             EntityToUpdateDefinition entityToUpdateDefinition)
            : base(joinKeys, GetConflictResolutionRule(syncDirection))
        {
            if (entityToUpdateDefinition == null)
                throw new Exception("The definition for the entity to update can not be null.");

            if (entityToUpdateDefinition.SyncSide == SyncSide.Target && syncDirection != SyncDirection.SourceToTarget)
                throw new Exception("Target-side entity can not be updated when sync direction is target -> source.");
            else if (entityToUpdateDefinition.SyncSide == SyncSide.Source && syncDirection != SyncDirection.TargetToSource)
                throw new Exception("Source-side entity can not be updated when sync direction is source -> target.");

            SyncDirection = syncDirection;

            EntityToUpdateDefinition = entityToUpdateDefinition;
        }

        private static Func<DataRow, ConflictResolutionResult> GetConflictResolutionRule(SyncDirection syncDirection)
        {
            switch (syncDirection)
            {
                case SyncDirection.SourceToTarget:
                    return (row) => { return ConflictResolutionResult.SourceWon; };

                case SyncDirection.TargetToSource:
                    return (row) => { return ConflictResolutionResult.TargetWon; };

                default:
                    throw new EnumValueNotImplementedException<SyncDirection>(syncDirection);
            } 
        }

        public void AddCompareField(string sourceField, string targetField, CompareAs compareAsType, Func<DataRow, object, object> preSaveConversionMethod)
        {
            switch (SyncDirection)
            {
                case SyncDirection.SourceToTarget:
                    AddCompareField(sourceField, targetField, targetField, compareAsType, preSaveConversionMethod);
                    break;

                case SyncDirection.TargetToSource:
                    AddCompareField(sourceField, targetField, sourceField, compareAsType, preSaveConversionMethod);
                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncDirection>(SyncDirection);
            } 
        }

        public void AddCompareField(string sourceFieldToCompare, string targetFieldToCompare, string fieldToUpdate, CompareAs compareAsType)
        {
            AddCompareField(sourceFieldToCompare, targetFieldToCompare, fieldToUpdate, compareAsType, null);
        }

        public void AddCompareField(string sourceFieldToCompare, string targetFieldToCompare, string fieldToUpdate,
                                    CompareAs compareAsType, Func<DataRow, object, object> preSaveConversionMethod)
        {
            switch (SyncDirection)
            {
                case SyncDirection.SourceToTarget:
                    base.AddCompareField(sourceFieldToCompare, sourceFieldToCompare, null, targetFieldToCompare, fieldToUpdate, preSaveConversionMethod, compareAsType);
                    break;

                case SyncDirection.TargetToSource:
                    base.AddCompareField(sourceFieldToCompare, fieldToUpdate, preSaveConversionMethod, targetFieldToCompare, targetFieldToCompare, null, compareAsType);
                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncDirection>(SyncDirection);
            }
        }

        public new void AddCompareField(string sourceField, string targetField, FieldMap fieldMap)
        {
            if (fieldMap is OneWayFieldMap)
            {
                var oneWayFieldMap = (OneWayFieldMap)fieldMap;

                oneWayFieldMap.SyncDirection = SyncDirection;

                base.AddCompareField(sourceField, targetField, oneWayFieldMap);
            }
            else if (fieldMap is TwoWayFieldMap)
            {
                var twoWayFieldMap = (TwoWayFieldMap)fieldMap;

                base.AddCompareField(sourceField, targetField, twoWayFieldMap);
            }
            else
                throw new DerivedClassNotImplementedException<FieldMap>(fieldMap);
        }

        public void AddCompareField(string sourceFieldToCompare, string targetFieldToCompare, string fieldToUpdate, FieldMap fieldMap)
        {
            if (fieldMap is OneWayFieldMap)
            {
                var oneWayFieldMap = (OneWayFieldMap)fieldMap;

                oneWayFieldMap.SyncDirection = SyncDirection;

                if (SyncDirection == SyncDirection.SourceToTarget)
                    base.AddCompareField(sourceFieldToCompare, sourceFieldToCompare, targetFieldToCompare, fieldToUpdate, fieldMap);
                else if (SyncDirection == SyncDirection.TargetToSource)
                    base.AddCompareField(sourceFieldToCompare, fieldToUpdate, targetFieldToCompare, targetFieldToCompare, fieldMap);
                else
                    throw new EnumValueNotImplementedException<SyncDirection>(SyncDirection);
            }
            else if (fieldMap is TwoWayFieldMap)
            {
                var twoWayFieldMap = (TwoWayFieldMap)fieldMap;

                if (SyncDirection == SyncDirection.SourceToTarget)
                    base.AddCompareField(sourceFieldToCompare, sourceFieldToCompare, targetFieldToCompare, fieldToUpdate, fieldMap);
                else if (SyncDirection == SyncDirection.TargetToSource)
                    base.AddCompareField(sourceFieldToCompare, fieldToUpdate, targetFieldToCompare, targetFieldToCompare, fieldMap);
                else
                    throw new EnumValueNotImplementedException<SyncDirection>(SyncDirection);
            }
            else
                throw new DerivedClassNotImplementedException<FieldMap>(fieldMap);
        }

        public void AddCompareField(string sourceField, string targetField, Func<DataRow, object, object, ComparisonResult> customCompareMethod, 
                                    Func<DataRow, object, object> preSaveConversionMethod)
        {
            switch (SyncDirection)
            {
                case SyncDirection.SourceToTarget:
                    AddCompareField(sourceField, targetField, targetField, customCompareMethod, preSaveConversionMethod);
                    break;

                case SyncDirection.TargetToSource:
                    AddCompareField(sourceField, targetField, sourceField, customCompareMethod, preSaveConversionMethod);
                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncDirection>(SyncDirection);
            } 
        }

        public void AddCompareField(string sourceFieldToCompare, string targetFieldToCompare, string fieldToUpdate,
                                    Func<DataRow, object, object, ComparisonResult> customCompareMethod, 
                                    Func<DataRow, object, object> preSaveConversionMethod)
        {
            switch (SyncDirection)
            {
                case SyncDirection.SourceToTarget:
                    base.AddCompareField(sourceFieldToCompare, sourceFieldToCompare, null, targetFieldToCompare, fieldToUpdate, preSaveConversionMethod, customCompareMethod);
                    break;

                case SyncDirection.TargetToSource:
                    base.AddCompareField(sourceFieldToCompare, fieldToUpdate, preSaveConversionMethod, targetFieldToCompare, targetFieldToCompare, null, customCompareMethod);
                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncDirection>(SyncDirection);
            }
        }

        public void AddCustomSetField(string fieldToUpdate, Func<DataRow, object> customSetMethod, SyncOperation appliesTo, bool onlyApplyWithOtherChanges = false)
        {
            AddCustomSetField(fieldToUpdate, fieldToUpdate, customSetMethod, appliesTo, onlyApplyWithOtherChanges);
        }

        public void AddCustomSetField(string fieldToCompare, string fieldToUpdate, Func<DataRow, object> customSetMethod, SyncOperation appliesTo, bool onlyApplyWithOtherChanges = false)
        {
            switch (SyncDirection)
            {
                case SyncDirection.SourceToTarget:
                    AddCustomSetField(SyncSide.Target, fieldToCompare, fieldToUpdate, customSetMethod, appliesTo, onlyApplyWithOtherChanges);
                    break;

                case SyncDirection.TargetToSource:
                    AddCustomSetField(SyncSide.Source, fieldToCompare, fieldToUpdate, customSetMethod, appliesTo, onlyApplyWithOtherChanges);
                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncDirection>(SyncDirection);
            }
        }

        public void AddAutoSetField(string fieldToUpdate, object value, SyncOperation appliesTo, bool onlyApplyWithOtherChanges = false)
        {
            AddAutoSetField(fieldToUpdate, fieldToUpdate, value, appliesTo, onlyApplyWithOtherChanges);
        }

        public void AddAutoSetField(string fieldToCompare, string fieldToUpdate, object value, SyncOperation appliesTo, bool onlyApplyWithOtherChanges = false)
        {
            switch (SyncDirection)
            {
                case SyncDirection.SourceToTarget:
                    AddCustomSetField(SyncSide.Target, fieldToCompare, fieldToUpdate, (row) => { return value; }, appliesTo, onlyApplyWithOtherChanges);
                    break;

                case SyncDirection.TargetToSource:
                    AddCustomSetField(SyncSide.Source, fieldToCompare, fieldToUpdate, (row) => { return value; }, appliesTo, onlyApplyWithOtherChanges);
                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncDirection>(SyncDirection);
            } 
        }

        public override HashSet<string> GetMappedDataFieldNames(SyncSide syncSide, DataMapFieldType includedFieldTypes = DataMapFieldType.All, string prefix = "")
        {
            HashSet<string> dataFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (syncSide == SyncSide.Source)
            {
                if (SyncDirection == Core.SyncDirection.TargetToSource)
                {
                    if (includedFieldTypes.HasFlag(DataMapFieldType.PrimaryKey) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                    {
                        foreach (var primaryKey in EntityToUpdateDefinition.PrimaryKeyColumnNames)
                        {
                            dataFields.Add(prefix + primaryKey);
                        }
                    }

                    if (includedFieldTypes.HasFlag(DataMapFieldType.SecondaryKey) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                    {
                        foreach (var secondaryKey in EntityToUpdateDefinition.SecondaryKeyColumnNames)
                        {
                            dataFields.Add(prefix + secondaryKey);
                        }
                    }

                    if (includedFieldTypes.HasFlag(DataMapFieldType.DataOnlyField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                    {
                        foreach (var dataOnlyField in EntityToUpdateDefinition.DataOnlyFields)
                        {
                            dataFields.Add(prefix + dataOnlyField.FieldName);
                        }
                    }
                }

                if (includedFieldTypes.HasFlag(DataMapFieldType.CompareField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                {
                    foreach (var compareField in CompareFields)
                    {
                        dataFields.Add(prefix + compareField.SourceFieldToCompare);
                    }
                }   
            }
            else if (syncSide == SyncSide.Target)
            {
                if (SyncDirection == Core.SyncDirection.SourceToTarget)
                {
                    if (includedFieldTypes.HasFlag(DataMapFieldType.PrimaryKey) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                    {
                        foreach (var primaryKey in EntityToUpdateDefinition.PrimaryKeyColumnNames)
                        {
                            dataFields.Add(prefix + primaryKey);
                        }
                    }

                    if (includedFieldTypes.HasFlag(DataMapFieldType.SecondaryKey) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                    {
                        foreach (var secondaryKey in EntityToUpdateDefinition.SecondaryKeyColumnNames)
                        {
                            dataFields.Add(prefix + secondaryKey);
                        }
                    }

                    if (includedFieldTypes.HasFlag(DataMapFieldType.DataOnlyField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                    {
                        foreach (var dataOnlyField in EntityToUpdateDefinition.DataOnlyFields)
                        {
                            dataFields.Add(prefix + dataOnlyField.FieldName);
                        }
                    }
                }

                if (includedFieldTypes.HasFlag(DataMapFieldType.CompareField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
                {
                    foreach (var compareField in CompareFields)
                    {
                        dataFields.Add(prefix + compareField.TargetFieldToCompare);
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