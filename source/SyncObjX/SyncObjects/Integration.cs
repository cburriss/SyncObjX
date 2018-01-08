using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class Integration : IntegrationDefinition
    {
        private Dictionary<string, string> _extendedProperties = new Dictionary<string, string>();

        private Assembly _packageAssembly;

        /* http://stackoverflow.com/questions/2562088/wcf-serializing-and-deserializing-generic-collections */
        [DataMember]
        private List<Job> _jobs = new List<Job>();

        public Assembly PackageAssembly
        {
            get
            {
                if (_packageAssembly == null)
                {
                    _packageAssembly = Assembly.LoadFrom(PackageDllPathAndFilename);
                }

                return _packageAssembly;
            }
        }

        [DataMember]
        public Dictionary<string, string> ExtendedProperties
        {
            get { return _extendedProperties; }
            set { _extendedProperties = value; }
        }

        [DataMember]
        public List<Job> Jobs
        {
            get { return _jobs; }
            private set { }
        }

        public Integration(Guid integrationId, string name, string packageDllDirectoryAndFilename, string sourceName, string targetName, bool isEnabled = true)
            : base (integrationId, name, packageDllDirectoryAndFilename, sourceName, targetName, isEnabled) { }

        public void AddJob(Job job)
        {
            if (_jobs.Where(d => d.Id == job.Id).Count() > 0)
                throw new Exception(string.Format("A job with ID '{0}' has already been added.", job.Id));
            else
                _jobs.Add(job);
        }

        public void AddJobs(IEnumerable<Job> jobs)
        {
            foreach (var job in jobs)
            {
                AddJob(job);
            }
        }
    }
}
