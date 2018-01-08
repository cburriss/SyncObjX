using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

namespace SyncObjX.Data
{
    [DataContract]
    public class TransposeDataOnlyField
    {
        [DataMember]
        public readonly string FieldName;

        public Func<IEnumerable<DataRow>, DataRow, string, string> MethodToPopulateValue;

        public TransposeDataOnlyField(string mappedFieldName, Func<IEnumerable<DataRow>, DataRow, string, string> methodToPopulateValue)
        {
            if (String.IsNullOrWhiteSpace(mappedFieldName))
                throw new Exception("Field name is missing or empty.");

            FieldName = mappedFieldName;

            MethodToPopulateValue = methodToPopulateValue;
        }
    }
}