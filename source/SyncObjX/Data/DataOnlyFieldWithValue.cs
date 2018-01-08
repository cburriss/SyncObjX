using System.Runtime.Serialization;

namespace SyncObjX.Data
{
    [DataContract]
    public class DataOnlyFieldWithValue : DataOnlyField
    {
        [DataMember]
        public string Value;

        public DataOnlyFieldWithValue(DataOnlyField dataOnlyField, string value)
            : base(dataOnlyField.FieldName, dataOnlyField.IsRequiredByJobBatch) 
        {
            Value = value;
        }
        
        public DataOnlyFieldWithValue(string fieldName, string value, bool isRequiredByJobBatch = false)
            : base(fieldName, isRequiredByJobBatch) 
        {
            Value = value;
        }
    }
}
