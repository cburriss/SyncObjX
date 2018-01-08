using System;
using System.Runtime.Serialization;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class JobStep : JobStepDefinition
    {  
        public JobStep(Guid jobStepId, string name, string fullyQualifiedName, bool isEnabled = true)
            : base(jobStepId, name, fullyQualifiedName, isEnabled) { }
    }
}
