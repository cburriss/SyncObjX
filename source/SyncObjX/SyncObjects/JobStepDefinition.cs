using System;
using System.Runtime.Serialization;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class JobStepDefinition : SyncObject
    {
        public static readonly bool IsEnabledDefault = true;

        public static readonly SyncObjectType SyncObjectType = SyncObjectType.Step;

        private string _name;

        private string _description;

        private string _fullyQualifiedName;

        private bool _isEnabled = IsEnabledDefault;

        [DataMember]
        public string Name
        {
            get { return _name; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Job step name is missing or empty.");
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
        public string FullyQualifiedName
        {
            get { return _fullyQualifiedName; }

            set
            {
                if (String.IsNullOrWhiteSpace(value) || !value.Contains("."))
                    throw new Exception("Job step fully qualified name is missing, empty, or invalid. Provide the fully qualified name of the job step class name, including namespace.");
                else
                    _fullyQualifiedName = value.Trim();
            }
        }

        [DataMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public JobStepDefinition(Guid jobStepId, string name, string fullyQualifiedName, bool isEnabled = true)
        {
            if (jobStepId == Guid.Empty)
                throw new Exception("Job step ID must be a valid GUID.");

            Id = jobStepId;

            Name = name;

            FullyQualifiedName = fullyQualifiedName;

            IsEnabled = isEnabled;
        }
    }
}
