using System;
using System.IO;
using System.Runtime.Serialization;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class AdapterDefinition : SyncObject
    {
        public static readonly SyncObjectType SyncObjectType = SyncObjectType.Adapter;

        private string _name;

        private string _description;

        private string _DllDirectory;

        private string _DllFilename;

        private string _fullyQualifiedName;

        [DataMember]
        public string Name
        {
            get { return _name; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Adapter name is missing or empty.");
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
        public string DllDirectory
        {
            get { return _DllDirectory; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Adapter assembly directory is missing or empty. Provide the directory path to a valid DLL.");

                _DllDirectory = value.Trim();
            }
        }

        [DataMember]
        public string DllFilename
        {
            get { return _DllFilename; }

            set
            {
                if (String.IsNullOrWhiteSpace(value) || !Path.GetExtension(value).ToLower().Equals(".dll"))
                    throw new Exception("Adapter assembly filename is missing, empty, or invalid. Provide the filename for valid DLL.");
                else
                    _DllFilename = value.Trim();
            }
        }

        [DataMember]
        public string DllPathAndFilename
        {
            get { return DllDirectory + "\\" + DllFilename; }

            private set {} // needed for serialization
        }

        [DataMember]
        public string FullyQualifiedName
        {
            get { return _fullyQualifiedName; }

            set
            {
                if (String.IsNullOrWhiteSpace(value) || !value.Contains("."))
                    throw new Exception("Adapter fully qualified name is missing, empty, or invalid. Provide the fully qualified name of the adapter class name, including namespace.");
                else
                    _fullyQualifiedName = value.Trim();
            }
        }

        public AdapterDefinition(Guid adapterId, string name, string dllDirectoryAndFilename, string fullyQualifiedName)
        {
            if (adapterId == Guid.Empty)
                throw new Exception("Adapter ID must be a valid GUID.");

            Id = adapterId;

            Name = name;

            DllDirectory = Path.GetDirectoryName(dllDirectoryAndFilename);

            DllFilename = Path.GetFileName(dllDirectoryAndFilename);

            FullyQualifiedName = fullyQualifiedName;
        }
    }
}
