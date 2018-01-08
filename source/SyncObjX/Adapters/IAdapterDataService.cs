using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using SyncObjX.Core;
using SyncObjX.Data;

namespace SyncObjX.Adapters
{
    [ServiceContract]
    public interface IAdapterDataService
    {
        [OperationContract]
        DataSet GetDataByDataSourceName(string dataSourceName, object commandObj);

        [OperationContract]
        DataSet GetDataByDataSourceId(Guid dataSourceId, object commandObj);

        [OperationContract]
        JobBatch ProcessBatchByDataSourceName(string dataSourceName, SyncSide syncSide, List<EntityBatch> entityBatches);

        [OperationContract]
        JobBatch ProcessBatchByDataSourceId(Guid dataSourceId, SyncSide syncSide, List<EntityBatch> entityBatches);

        [OperationContract]
        object ExecuteCommandByDataSourceName(string dataSourceName, object commandObj);

        [OperationContract]
        object ExecuteCommandByDataSourceId(Guid dataSourceId, object commandObj);
    }
}
