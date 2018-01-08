using SyncObjX.Data;

namespace SyncObjX.Core
{
    public interface IOneWayDataMap
    {
        SyncDirection SyncDirection { get; }

        EntityToUpdateDefinition EntityToUpdateDefinition { get; }
    }
}
