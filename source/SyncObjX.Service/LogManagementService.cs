using System;
using SyncObjX.Configuration;
using SyncObjX.Logging;

namespace SyncObjX.Service
{
    public class LogManagementService : ISyncEngineLogManagement
    {
        private SyncEngineDatabaseConfigurator configurator;

        private const string WEB_SERVICE_EXCEPTION_MESSAGE = "An exception occurred within the log management web service.";

        public LogManagementService()
        {
            configurator = new SyncEngineDatabaseConfigurator(IntegrationWindowsService.IntegrationDbConnectionString);
        }

        public void DeleteJobDataSourceHistory(Guid jobId)
        {
            try
            {
                SyncEngineLogger.DeleteJobDataSourceHistory(jobId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteIntegrationHistory(Guid integrationId, int daysToKeep)
        {
            try
            {
                SyncEngineLogger.DeleteIntegrationHistory(integrationId, daysToKeep);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteServiceHistory(int daysToKeep)
        {
            try
            {
                SyncEngineLogger.DeleteServiceHistory(daysToKeep);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteAllHistory()
        {
            try
            {
                SyncEngineLogger.DeleteAllHistory();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }
    }
}
