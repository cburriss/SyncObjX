using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class Adapter : AdapterDefinition
    {
        private Dictionary<string, string> _extendedProperties = new Dictionary<string, string>();

        [DataMember]
        public Dictionary<string, string> ExtendedProperties
        {
            get { return _extendedProperties; }
            set { _extendedProperties = value; }
        }

        public Adapter(Guid adapterId, string name, string dllDirectoryAndFilename, string fullyQualifiedName)
            : base(adapterId, name, dllDirectoryAndFilename, fullyQualifiedName) { }
    }
}
