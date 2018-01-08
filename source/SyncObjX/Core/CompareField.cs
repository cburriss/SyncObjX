using System;
using System.Data;

namespace SyncObjX.Core
{
    public class CompareField
    {
        public readonly CompareType Type;

        public readonly CompareAs CompareAs;

        public readonly FieldMap FieldMap;

        public readonly Func<DataRow, object, object, ComparisonResult> CustomCompareMethod;

        string _sourceFieldToCompare;

        string _sourceFieldToUpdate;

        string _sourceFieldToCompareWithPrefix;

        private Func<DataRow, object, object> _sourcePreSaveConversionMethod;

        string _targetFieldToCompare;

        string _targetFieldToUpdate;

        string _targetFieldToCompareWithPrefix;

        private Func<DataRow, object, object> _targetPreSaveConversionMethod;

        public string SourceFieldToCompare
        {
            get { return _sourceFieldToCompare; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("SourceFieldToCompare is missing or empty.");
                else
                    _sourceFieldToCompare = value.Trim();
            }
        }

        public string SourceFieldToUpdate
        {
            get { return _sourceFieldToUpdate; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("SourceFieldToUpdate is missing or empty.");
                else
                    _sourceFieldToUpdate = value.Trim();
            }
        }

        public string SourceFieldToCompareWithPrefix
        {
            get { return _sourceFieldToCompareWithPrefix; }
        }

        /// <summary>
        /// Executes method against source value for conversion before applying updates.
        /// </summary>
        public Func<DataRow, object, object> SourcePreSaveConversionMethod
        {
            get { return _sourcePreSaveConversionMethod; }
            set { _sourcePreSaveConversionMethod = value; }
        }

        public string TargetFieldToCompare
        {
            get { return _targetFieldToCompare; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("TargetFieldToCompare is missing or empty.");
                else
                    _targetFieldToCompare = value.Trim();
            }
        }

        public string TargetFieldToUpdate
        {
            get { return _targetFieldToUpdate; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("TargetFieldToUpdate is missing or empty.");
                else
                    _targetFieldToUpdate = value.Trim();
            }
        }

        public string TargetFieldToCompareWithPrefix
        {
            get { return _targetFieldToCompareWithPrefix; }
        }

        /// <summary>
        /// Executes method against target value for conversion before applying updates.
        /// </summary>
        public Func<DataRow, object, object> TargetPreSaveConversionMethod
        {
            get { return _targetPreSaveConversionMethod; }
            set { _targetPreSaveConversionMethod = value; }
        }

        public CompareField(string sourceField, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                            string targetField, Func<DataRow, object, object> targetPreSaveConversionMethod, 
                            CompareAs compareAsType)
            : this(sourceField, sourceField, sourcePreSaveConversionMethod, targetField, targetField, targetPreSaveConversionMethod, compareAsType) { }

        public CompareField(string sourceFieldToCompare, string sourceFieldToUpdate, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                            string targetFieldToCompare, string targetFieldToUpdate, Func<DataRow, object, object> targetPreSaveConversionMethod,
                            CompareAs compareAsType)
        {
            Type = CompareType.ConvertToType;

            CompareAs = compareAsType;

            SetFields(sourceFieldToCompare, sourceFieldToUpdate, targetFieldToCompare, targetFieldToUpdate);

            _sourcePreSaveConversionMethod = sourcePreSaveConversionMethod;

            _targetPreSaveConversionMethod = targetPreSaveConversionMethod;
        }

        public CompareField(string sourceField, string targetField, FieldMap fieldMap)
            : this(sourceField, sourceField, targetField, targetField, fieldMap) { }

        public CompareField(string sourceFieldToCompare, string sourceFieldToUpdate, string targetFieldToCompare, string targetFieldToUpdate, FieldMap fieldMap)
        {
            Type = CompareType.FieldMap;

            FieldMap = fieldMap;

            SetFields(sourceFieldToCompare, sourceFieldToUpdate, targetFieldToCompare, targetFieldToUpdate);
        }

        public CompareField(string sourceField, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                            string targetField, Func<DataRow, object, object> targetPreSaveConversionMethod,
                            Func<DataRow, object, object, ComparisonResult> customCompareMethod)
            : this(sourceField, sourceField, sourcePreSaveConversionMethod, targetField, targetField, targetPreSaveConversionMethod, customCompareMethod) { }

        public CompareField(string sourceFieldToCompare, string sourceFieldToUpdate, Func<DataRow, object, object> sourcePreSaveConversionMethod,
                            string targetFieldToCompare, string targetFieldToUpdate, Func<DataRow, object, object> targetPreSaveConversionMethod,
                            Func<DataRow, object, object, ComparisonResult> customCompareMethod)
        {
            Type = CompareType.CustomMethod;

            CustomCompareMethod = customCompareMethod;

            SetFields(sourceFieldToCompare, sourceFieldToUpdate, targetFieldToCompare, targetFieldToUpdate);

            _sourcePreSaveConversionMethod = sourcePreSaveConversionMethod;

            _targetPreSaveConversionMethod = targetPreSaveConversionMethod;
        }

        private void SetFields(string sourceFieldToCompare, string sourceFieldToUpdate, string targetFieldToCompare, string targetFieldToUpdate)
        {
            SourceFieldToCompare = sourceFieldToCompare;
            SourceFieldToUpdate = sourceFieldToUpdate;

            TargetFieldToCompare = targetFieldToCompare;
            TargetFieldToUpdate = targetFieldToUpdate;

            _sourceFieldToCompareWithPrefix = DataTableHelper.SOURCE_PREFIX + SourceFieldToCompare;
            _targetFieldToCompareWithPrefix = DataTableHelper.TARGET_PREFIX + TargetFieldToCompare;
        }
    }
}
