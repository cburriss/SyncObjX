using System.Data;
using SyncObjX.Data;

namespace SyncObjX.Adapters
{
    public interface IAdapter
    {
        DataSet GetData(object commandObj);

        void ProcessBatch(JobBatch batch);

        object ExecuteCommand(object commandObj);
    }
}
