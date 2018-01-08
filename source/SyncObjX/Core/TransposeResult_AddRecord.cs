using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncObjX.Core
{
    public class TransposeResult_AddRecord : TransposeResult
    {
        public readonly Dictionary<string, string> FieldValuePairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public TransposeResult_AddRecord() { }

        public TransposeResult_AddRecord(string fieldName, string newValue)
        {
            if (String.IsNullOrWhiteSpace(fieldName))
                throw new Exception("Field name is missing or empty.");

            FieldValuePairs.Add(fieldName, newValue);
        }

        public TransposeResult_AddRecord (IEnumerable<KeyValuePair<string, string>> fieldValuePairs)
	    {
            if (fieldValuePairs == null || fieldValuePairs.Count() == 0)
                throw new Exception("At least one field is required.");

            foreach (var fieldValuePair in fieldValuePairs)
            {
                if (String.IsNullOrWhiteSpace(fieldValuePair.Key))
                    throw new Exception("Field name is missing or empty.");

                FieldValuePairs.Add(fieldValuePair.Key, fieldValuePair.Value);
            }
	    }

        public void AddFieldValuePair(string fieldName, string value)
        {
            if (FieldValuePairs.ContainsKey(fieldName))
                throw new Exception("Field name already exists.");
            else
                FieldValuePairs.Add(fieldName, value);
        }     
    }
}
