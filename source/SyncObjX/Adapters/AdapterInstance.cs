using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SyncObjX.Configuration;
using SyncObjX.Data;
using SyncObjX.SyncObjects;

namespace SyncObjX.Adapters
{
    public abstract class AdapterInstance : IAdapter, IDisposable
    {
        Dictionary<string, object> _properties = new Dictionary<string, object>();

        public Dictionary<string, object> Properties
        {
            get { return _properties; }
        }

        public readonly DataSource AssociatedDataSource;

        public AdapterInstance(Dictionary<string, object> properties, DataSource associatedDataSource)
        {
            AssociatedDataSource = associatedDataSource;

            if (properties == null)
                properties = new Dictionary<string,object>();

            // convert to case insensitve
            var supportedProperties = GetSupportedProperties().ToDictionary(d => d.Key, d => d.Value, StringComparer.OrdinalIgnoreCase);
            var adapterConfigProperties = properties.ToDictionary(d => d.Key, d => d.Value, StringComparer.OrdinalIgnoreCase);

            var requiredProperties = supportedProperties.Values.Where(d => d.IsRequired);

            // check if required properties were provided
            foreach (var requiredProp in requiredProperties)
            {
                if (!adapterConfigProperties.ContainsKey(requiredProp.Name))
                    throw new Exception(string.Format("Required adapter property '{0}' is missing.", requiredProp.Name));
            }

            // check if non-supported adapter properties are provided
            foreach (var adapterProp in adapterConfigProperties)
            {
                if (!supportedProperties.ContainsKey(adapterProp.Key))
                    throw new Exception(string.Format("Property '{0}' is not supported by this adapter.", adapterProp.Key));
            }

            // add properties to adapter; if not provided and not required, add with default value
            foreach(var supportedProperty in supportedProperties)
            {
                if (adapterConfigProperties.ContainsKey(supportedProperty.Value.Name))
                {
                    SupportedProperty.ThrowExceptionIfInvalid(supportedProperty.Value, adapterConfigProperties[supportedProperty.Value.Name]);
                    _properties.Add(supportedProperty.Value.Name, adapterConfigProperties[supportedProperty.Value.Name]);
                }
                else
                {
                    _properties.Add(supportedProperty.Value.Name, supportedProperty.Value.DefaultValue);
                }
            }
        }

        public abstract Dictionary<string, SupportedProperty> GetSupportedProperties();

        public abstract DataSet GetData(object commandObj);

        public abstract void ProcessBatch(JobBatch jobBatch);

        public abstract object ExecuteCommand(object commandObj);

        public abstract void Dispose();
    }
}
