using System.Collections.Generic;
using SyncObjX.Configuration;

namespace SyncObjX.Adapters
{
    public interface IAdapterConfig
    {
        Dictionary<string, SupportedProperty> GetSupportedProperties();
    }
}