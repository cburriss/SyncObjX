using System;
using System.ServiceModel;

namespace SyncObjX.Logging
{
    [ServiceContract]
    public interface ISyncEngineLogManagement
    {
        [OperationContract]
        void DeleteJobDataSourceHistory(Guid jobId);

        [OperationContract]
        void DeleteIntegrationHistory(Guid integrationId, int daysToKeep);

        [OperationContract]
        void DeleteServiceHistory(int daysToKeep);

        [OperationContract]
        void DeleteAllHistory();
    }
}
