using System;
using System.Runtime.Serialization;
using SyncObjX.Core;

namespace SyncObjX.Management
{
    [DataContract]
    public class JobFilter
    {
        [DataMember]
        public SyncSide SyncSide;

        [DataMember]
        public string FieldName;

        [DataMember]
        public JobFilterOperator Operator;

        [DataMember]
        public object Value;

        [DataMember]
        public bool EncloseValueInSingleQuotes;

        public JobFilter(SyncSide syncSide, string fieldName, JobFilterOperator @operator, object value, bool encloseValueInSingleQuotes = true)
        {
            if (String.IsNullOrWhiteSpace(fieldName))
                throw new Exception("Field name is missing or empty.");

            SyncSide = syncSide;

            FieldName = fieldName;

            Operator = @operator;

            Value = value;

            EncloseValueInSingleQuotes = encloseValueInSingleQuotes;
        }
    }
}
