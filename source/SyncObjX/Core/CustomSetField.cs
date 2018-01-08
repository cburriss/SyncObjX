using System;
using System.Data;

namespace SyncObjX.Core
{
    public class CustomSetField
    {
        public readonly SyncSide SyncSide;

        public readonly string FieldNameToCompare;

        public readonly string FieldNameToCompareWithPrefix;

        public readonly string FieldNameToUpdate;

        public readonly string FieldNameToUpdateWithPrefix;

        public readonly Func<DataRow, Object> CustomSetMethod;

        public readonly SyncOperation AppliesTo;

        public readonly bool OnlyApplyWithOtherChanges;

        public CustomSetField(SyncSide syncSide, string fieldNameToUpdate, Func<DataRow, Object> customSetMethod, SyncOperation appliesTo, bool onlyApplyWithOtherChanges = false)
            : this (syncSide, fieldNameToUpdate, fieldNameToUpdate, customSetMethod, appliesTo, onlyApplyWithOtherChanges) { }

        public CustomSetField(SyncSide syncSide, string fieldNameToCompare, string fieldNameToUpdate, Func<DataRow, Object> customSetMethod, SyncOperation appliesTo, bool onlyApplyWithOtherChanges = false)
        {
            if (String.IsNullOrWhiteSpace(fieldNameToCompare))
                throw new Exception("Field name to compare is missing or empty.");

            if (String.IsNullOrWhiteSpace(fieldNameToUpdate))
                throw new Exception("Field name to update is missing or empty.");

            if (customSetMethod == null)
                throw new Exception("Custom method can not be null.");

            SyncSide = syncSide;

            FieldNameToCompare = fieldNameToCompare;

            FieldNameToUpdate = fieldNameToUpdate;

            if (SyncSide == Core.SyncSide.Source)
            {
                FieldNameToCompareWithPrefix = DataTableHelper.SOURCE_PREFIX + fieldNameToCompare;
                FieldNameToUpdateWithPrefix = DataTableHelper.SOURCE_PREFIX + fieldNameToUpdate;
            }
            else
            {
                FieldNameToCompareWithPrefix = DataTableHelper.TARGET_PREFIX + fieldNameToCompare;
                FieldNameToUpdateWithPrefix = DataTableHelper.TARGET_PREFIX + fieldNameToUpdate;
            }

            CustomSetMethod = customSetMethod;

            AppliesTo = appliesTo;

            OnlyApplyWithOtherChanges = onlyApplyWithOtherChanges;
        }
    }
}