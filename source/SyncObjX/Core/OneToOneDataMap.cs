using System;
using System.Collections.Generic;
using System.Data;

namespace SyncObjX.Core
{
    public abstract class OneToOneDataMap : DataMap
    {
        public Func<DataRow, ConflictResolutionResult> ConflictResolutionRule;

        protected List<CompareField> _compareFields = new List<CompareField>();

        public IEnumerable<CompareField> CompareFields
        {
            get { return _compareFields; }
        }

        public OneToOneDataMap(JoinFieldCollection joinKeys, Func<DataRow, ConflictResolutionResult> conflictResolutionRule)
            : base(joinKeys)
        {
            if (conflictResolutionRule == null)
                throw new Exception("Method for conflict resolution can not be null.");

            ConflictResolutionRule = conflictResolutionRule;
        }

        public void AddCompareField(string sourceField, string targetField, CompareAs compareAsType)
        {
            AddCompareField(sourceField, sourceField, null, targetField, targetField, null, compareAsType);
        }

        protected void AddCompareField(string sourceField, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                                       string targetField, Func<DataRow, object, object> targetPreSaveConversionMethod,
                                       CompareAs compareAsType)
        {
            AddCompareField(sourceField, sourceField, sourcePreSaveConversionMethod, targetField, targetField, targetPreSaveConversionMethod, compareAsType);
        }

        protected void AddCompareField(string sourceFieldToCompare, string sourceFieldToUpdate, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                                       string targetFieldToCompare, string targetFieldToUpdate, Func<DataRow, object, object> targetPreSaveConversionMethod,
                                       CompareAs compareAsType)
        {
            AddToSyncSideFieldsList(SyncSide.Source, sourceFieldToUpdate);
            AddToSyncSideFieldsList(SyncSide.Target, targetFieldToUpdate);

            _compareFields.Add(new CompareField(sourceFieldToCompare, sourceFieldToUpdate, sourcePreSaveConversionMethod, 
                                                targetFieldToCompare, targetFieldToUpdate, targetPreSaveConversionMethod, compareAsType));
        }

        protected void AddCompareField(string sourceField, string targetField, FieldMap fieldMap)
        {
            AddCompareField(sourceField, sourceField, targetField, targetField, fieldMap);
        }

        protected void AddCompareField(string sourceFieldToCompare, string sourceFieldToUpdate,
                                       string targetFieldToCompare, string targetFieldToUpdate,
                                       FieldMap fieldMap)
        {
            AddToSyncSideFieldsList(SyncSide.Source, sourceFieldToUpdate);
            AddToSyncSideFieldsList(SyncSide.Target, targetFieldToUpdate);

            _compareFields.Add(new CompareField(sourceFieldToCompare, sourceFieldToUpdate, targetFieldToCompare, targetFieldToUpdate, fieldMap));
        }

        protected void AddCompareField(string sourceField, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                                       string targetField, Func<DataRow, object, object> targetPreSaveConversionMethod,
                                       Func<DataRow, object, object, ComparisonResult> customCompareMethod)
        {
            AddCompareField(sourceField, sourceField, sourcePreSaveConversionMethod, targetField, targetField, targetPreSaveConversionMethod, customCompareMethod);
        }

        protected void AddCompareField(string sourceFieldToCompare, string sourceFieldToUpdate, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                                       string targetFieldToCompare, string targetFieldToUpdate, Func<DataRow, object, object> targetPreSaveConversionMethod,
                                       Func<DataRow, object, object, ComparisonResult> customCompareMethod)
        {
            AddToSyncSideFieldsList(SyncSide.Source, sourceFieldToUpdate);
            AddToSyncSideFieldsList(SyncSide.Target, targetFieldToUpdate);

            _compareFields.Add(new CompareField(sourceFieldToCompare, sourceFieldToUpdate, sourcePreSaveConversionMethod,
                                                targetFieldToCompare, targetFieldToUpdate, targetPreSaveConversionMethod, customCompareMethod));
        }
    }
}
