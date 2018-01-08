using System;
using System.Runtime.Serialization;
using SyncObjX.Logging;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class DataSourceDefinition : SyncObject
    {
        public static readonly SyncObjectType SyncObjectType = SyncObjectType.DataSource;

        public static readonly LoggingLevel LoggingLevelDefault = LoggingLevel.InheritFromParent;

        private string _name;

        private string _description;

        private Adapter _adapter;

        private LoggingLevel _loggingLevel = LoggingLevelDefault;

        [DataMember]
        public string Name
        {
            get { return _name; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Data source name is missing or empty.");
                else
                    _name = value.Trim();
            }
        }

        [DataMember]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [DataMember]
        public Adapter Adapter
        {
            get { return _adapter; }

            set
            {
                if (value == null)
                    throw new Exception("Adapter cannot be null.");
                else
                    _adapter = value;
            }
        }

        [DataMember]
        public LoggingLevel LoggingLevel
        {
            get { return _loggingLevel; }
            set { _loggingLevel = value; }
        }

        public DataSourceDefinition(Guid dataSourceId, string name, Adapter adapter)
        {
            if (dataSourceId == Guid.Empty)
                throw new Exception("Data source ID must be a valid GUID.");

            Id = dataSourceId;

            Name = name;

            Adapter = adapter;
        }
    }
}
