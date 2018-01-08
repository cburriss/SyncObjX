using System;
using System.Runtime.Serialization;

namespace SyncObjX.Data
{
    [DataContract]
    public class DataFilter
    {
        [DataMember]
        public string FieldName;

        [DataMember]
        public DataFilterOperator Operator;

        [DataMember]
        public object Value;

        [DataMember]
        public bool EncloseValueInSingleQuotes;

        // added to get around annoying WCF service issue - DataFilter does not contain constructor that accepts 0 arguments
        public DataFilter() { }

        public DataFilter(string fieldName, DataFilterOperator @operator, object value, bool encloseValueInSingleQuotes = true)
        {
            if (String.IsNullOrWhiteSpace(fieldName))
                throw new Exception("Field name is missing or empty.");

            FieldName = fieldName;

            Operator = @operator;

            Value = value;

            EncloseValueInSingleQuotes = encloseValueInSingleQuotes;
        }
    }
}
