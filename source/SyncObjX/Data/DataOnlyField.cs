using System;
using System.Data;
using System.Runtime.Serialization;

namespace SyncObjX.Data
{
    [DataContract]
    public class DataOnlyField
    {
        [DataMember]
        public readonly string FieldName;

        [DataMember]
        public bool IsRequiredByJobBatch;

        public Func<DataRow, object> MethodToPopulateValue;

        public DataOnlyField(string mappedFieldName, bool isRequiredByJobBatch = false)
            : this (mappedFieldName, null, isRequiredByJobBatch) { }

        public DataOnlyField(string fieldIdentifier, Func<DataRow, object> methodToPopulateValue, bool isRequiredByJobBatch = false)
        {
            if (String.IsNullOrWhiteSpace(fieldIdentifier))
                throw new Exception("Field name is missing or empty.");

            FieldName = fieldIdentifier;

            MethodToPopulateValue = methodToPopulateValue;

            IsRequiredByJobBatch = isRequiredByJobBatch;
        }
    }
}
