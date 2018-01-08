using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SyncObjX.Data;
using SyncObjX.Exceptions;

namespace SyncObjX.Core
{
    public class OneToMany_OneWayDataMap : DataMap, IOneWayDataMap
    {
        public override DataTransferType TransferType
        {
            get { return DataTransferType.Unidirectional; }
        }

        public SyncDirection SyncDirection { get; private set; }

        public EntityToUpdateDefinition EntityToUpdateDefinition { get; private set; }

        public readonly HashSet<string> ColumnNamesToTranspose;

        /// <summary>
        /// <para>Iterates over all rows on the "one" side within the one-to-many relationship, then for each transposed column.</para>
        /// <para>IEnumerable<DataRow> - collection of records that have a key match to the opposing side's single record.</para>
        /// <para>DataRow - single record that has a key match to the opposing side's set of records.</para>
        /// <para>String - the transposed columnd name.</para>
        /// <para>Object - the transposed column's value.</para>
        /// </summary>
        public readonly Func<IEnumerable<DataRow>, DataRow, string, TransposeResult> TransposeMethod;

        public OneToMany_OneWayDataMap(SyncDirection syncDirection, JoinFieldCollection joinKeys, 
                                       EntityToUpdateDefinition entityToUpdateDefinition, HashSet<string> columnNamesToTranspose,
                                       Func<IEnumerable<DataRow>, DataRow, string, TransposeResult> transposeMethod)
            : base(joinKeys)
        {
            if (entityToUpdateDefinition == null)
                throw new Exception("The definition for the entity to update can not be null.");

            if (entityToUpdateDefinition.SyncSide == SyncSide.Target && syncDirection != SyncDirection.SourceToTarget)
                throw new Exception("Target-side entity can not be updated when sync direction is target -> source.");
            else if (entityToUpdateDefinition.SyncSide == SyncSide.Source && syncDirection != SyncDirection.TargetToSource)
                throw new Exception("Source-side entity can not be updated when sync direction is source -> target.");

            if (columnNamesToTranspose == null || columnNamesToTranspose.Count == 0)
                throw new Exception("At least one column name to transpose is required.");

            if (transposeMethod == null)
                throw new Exception("Transpose method can not be null.");

            SyncDirection = syncDirection;

            EntityToUpdateDefinition = entityToUpdateDefinition;

            ColumnNamesToTranspose = columnNamesToTranspose;

            TransposeMethod = transposeMethod;
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
                }
            }
            else
                throw new EnumValueNotImplementedException<SyncSide>(syncSide);

            if (includedFieldTypes.HasFlag(DataMapFieldType.DataOnlyField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
            {
                foreach (var dataOnlyField in EntityToUpdateDefinition.DataOnlyFields)
                {
                    dataFields.Add(prefix + dataOnlyField.FieldName);
                }
            }

            if (includedFieldTypes.HasFlag(DataMapFieldType.CustomSetField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
            {
                foreach (var customSetField in CustomSetFields.Where(d => d.SyncSide == syncSide))
                {
                    dataFields.Add(prefix + customSetField.FieldNameToCompare);
                }
            }

            if (includedFieldTypes.HasFlag(DataMapFieldType.TransposeField) || includedFieldTypes.HasFlag(DataMapFieldType.All))
            {
                foreach (var transposeColumnName in ColumnNamesToTranspose)
                {
                    dataFields.Add(prefix + transposeColumnName);
                }
            }

            return dataFields;
        }
    }
}
