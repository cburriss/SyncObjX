using System;
using System.Runtime.Serialization;
using SyncObjX.Logging;
using SyncObjX.Management;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class JobDefinition : SyncObject
    {
        public static readonly JobPriority PriorityDefault = JobPriority.Normal;

        public static readonly JobTerminateOnErrorType TerminateOnErrorTypeDefault = JobTerminateOnErrorType.TerminateExecutingDataSourceOnly;

        public static readonly LoggingLevel LoggingLevelDefault = LoggingLevel.InheritFromParent;

        public static readonly SyncObjectType SyncObjectType = SyncObjectType.Job;

        private string _name;

        private string _description;

        private bool _isEnabled;

        private JobPriority _priority = PriorityDefault;

        private JobTerminateOnErrorType _terminateOnErrorType = TerminateOnErrorTypeDefault;

        private LoggingLevel _loggingLevel = LoggingLevelDefault;

        [DataMember]
        public string Name
        {
            get { return _name; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Job name is missing or empty.");
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
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        [DataMember]
        public JobPriority Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        [DataMember]
        public JobTerminateOnErrorType TerminateOnErrorType
        {
            get { return _terminateOnErrorType; }
            set { _terminateOnErrorType = value; }
        }

        [DataMember]
        public LoggingLevel LoggingLevel
        {
            get { return _loggingLevel; }
            set { _loggingLevel = value; }
        }

        public JobDefinition(Guid jobId, string name, bool isEnabled = true)
        {
            if (jobId == Guid.Empty)
                throw new Exception("Job ID must be a valid GUID.");

            Id = jobId;

            Name = name;

            IsEnabled = isEnabled;
        }
    }
}