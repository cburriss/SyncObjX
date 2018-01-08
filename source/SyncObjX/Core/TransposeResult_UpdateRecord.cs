using System;
using System.Collections.Generic;
using System.Data;

namespace SyncObjX.Core
{
    public class TransposeResult_UpdateRecord : TransposeResult
    {
        public readonly List<string> PrimaryKeyValues;

        public readonly Dictionary<string, string> FieldValuePairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public readonly DataRow AssociatedManySideDataRow;
            
        public TransposeResult_UpdateRecord(DataRow associatedManySideDataRow, string primaryKeyValue)
            : this(associatedManySideDataRow, new List<string>() { primaryKeyValue }) { }

        public TransposeResult_UpdateRecord(DataRow associatedManySideDataRow, List<string> primaryKeyValues)
        {
            if (associatedManySideDataRow == null)
                throw new Exception("Associated many-side data row is required.");

            if (primaryKeyValues == null || primaryKeyValues.Count == 0)
                throw new Exception("At least for primary key value is required.");

            AssociatedManySideDataRow = associatedManySideDataRow;

            PrimaryKeyValues = primaryKeyValues;
        }

        public TransposeResult_UpdateRecord(DataRow associatedManySideDataRow, string primaryKeyValue, string fieldName, string newValue)
            : this(associatedManySideDataRow, new List<string>() { primaryKeyValue }, fieldName, newValue) { }

        public TransposeResult_UpdateRecord(DataRow associatedManySideDataRow, List<string> primaryKeyValues, string fieldName, string newValue)
            : this(associatedManySideDataRow, primaryKeyValues, new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(fieldName, newValue) }) { }

        public TransposeResult_UpdateRecord(DataRow associatedManySideDataRow, string primaryKeyValue, IEnumerable<KeyValuePair<string, string>> fieldValuePairs)
            : this(associatedManySideDataRow, new List<string>() { primaryKeyValue }, fieldValuePairs) { }

        public TransposeResult_UpdateRecord(DataRow associatedManySideDataRow, List<string> primaryKeyValues, IEnumerable<KeyValuePair<string, string>> fieldValuePairs)
	    {
            if (associatedManySideDataRow == null)
                throw new Exception("Associated many-side data row is required.");

            if (primaryKeyValues == null || primaryKeyValues.Count == 0)
                throw new Exception("At least for primary key value is required.");

            PrimaryKeyValues = primaryKeyValues;

            if (fieldValuePairs != null)
            {
                foreach (var fieldValuePair in fieldValuePairs)
                {
                    if (String.IsNullOrWhiteSpace(fieldValuePair.Key))
                        throw new Exception("Field name is missing or empty.");

                    FieldValuePairs.Add(fieldValuePair.Key, fieldValuePair.Value);
                }
            }

            AssociatedManySideDataRow = associatedManySideDataRow;
	    }

        public void AddFieldValuePair(string fieldName, string value)
        {
            if (FieldValuePairs.ContainsKey(fieldName))
                throw new Exception(string.Format("Field name '{0}' has already been added.", fieldName));
            else
                FieldValuePairs.Add(fieldName, value);
        }
    }
}
