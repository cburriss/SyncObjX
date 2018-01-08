using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using SyncObjX.Adapters;
using SyncObjX.Core;
using SyncObjX.Data;
using SyncObjX.Logging;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class DataSource : DataSourceDefinition, IAdapter
    {
        private Dictionary<string, string> _extendedProperties = new Dictionary<string, string>();

        [DataMember]
        public Dictionary<string, string> ExtendedProperties
        {
            get { return _extendedProperties; }
            set { _extendedProperties = value; }
        }

        public DataSource(Guid dataSourceId, string name, Adapter adapter)
            : base(dataSourceId, name, adapter) { }

        public DataSet GetData(object commandObj)
        {
            if (commandObj == null)
                throw new Exception("Command object can not be null.");

            using (var adapter = GetAdapterInstance(Adapter))
            {
                return adapter.GetData(commandObj);
            }
        }

        public void ProcessBatch(JobBatch jobBatch)
        {
            if (jobBatch == null)
                throw new Exception("Job batch can not be null.");

            if (jobBatch.EntityBatches == null || jobBatch.EntityBatches.Count == 0)
            {
                SyncEngineLogger.WriteByParallelTaskContext(LogEntryType.Info, () =>
                {

                    return string.Format("The job batch for sync side '{0}' and data source '{1}' contains no entity batches. No processing has occurred.",
                                        Enum.GetName(typeof(SyncSide), jobBatch.SyncSide), jobBatch.AssociatedDataSource.DataSource.Name);
                });
            }
            else
            {
                using (var adapter = GetAdapterInstance(Adapter))
                {
                    adapter.ProcessBatch(jobBatch);
                }
            }
        }

        public object ExecuteCommand(object commandObj)
        {
            if (commandObj == null)
                throw new Exception("Command object can not be null.");

            using (var adapter = GetAdapterInstance(Adapter))
            {
                return adapter.ExecuteCommand(commandObj);
            }
        }

        public AdapterInstance GetAdapterInstance(Adapter adapter)
        {
            if (!File.Exists(adapter.DllPathAndFilename))
                throw new Exception(string.Format("Adapter does not exist at location '{0}'.", adapter.DllPathAndFilename));

            var adapterAssembly = Assembly.LoadFrom(adapter.DllPathAndFilename);

            var mergedAdapterProperties = GetMergedAdapterProperties();

            var adapterType = adapterAssembly.GetType(adapter.FullyQualifiedName);

            if (adapterType == null)
                throw new Exception(string.Format("Adapter type with fully qualified name '{0}' could not be found in assembly '{1}'.", 
                                                    adapter.FullyQualifiedName, adapter.DllPathAndFilename));

            var adapterObj = Activator.CreateInstance(adapterType, mergedAdapterProperties, this);

            return (AdapterInstance)adapterObj;
        }

        /// <summary>
        /// Combines global properties defined for the adapter with datasource-specific adapter properties.
        /// 
        /// Datasource-specific adapter properties override global properties.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetMergedAdapterProperties()
        {
            Dictionary<string, object> mergedAdapterProperties = new Dictionary<string, object>();

            // start with the global properties
            foreach (var property in Adapter.ExtendedProperties)
            {
                mergedAdapterProperties.Add(property.Key, property.Value);
            }

            // merge the datasource-specific adapter properties
            foreach (var property in this.ExtendedProperties)
            {
                // if the property is defined for the datasource as well, override the global property
                if (mergedAdapterProperties.ContainsKey(property.Key))
                    mergedAdapterProperties[property.Key] = property.Value;
                // otherwise, the property is only defined at the datasource level
                else
                    mergedAdapterProperties.Add(property.Key, property.Value);
            }

            return mergedAdapterProperties;
        }
    }
}
