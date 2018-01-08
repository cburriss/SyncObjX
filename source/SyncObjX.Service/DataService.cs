using System;
using System.Collections.Generic;
using System.Data;
using SyncObjX.Adapters;
using SyncObjX.Configuration;
using SyncObjX.Core;
using SyncObjX.Data;
using SyncObjX.Logging;
using SyncObjX.SyncObjects;

namespace SyncObjX.Service
{
    public class DataService : IAdapterDataService
    {
        private SyncEngineDatabaseConfigurator configurator;

        private const string WEB_SERVICE_EXCEPTION_MESSAGE = "An exception occurred within the data web service.";

        public DataService()
        {
            configurator = new SyncEngineDatabaseConfigurator(IntegrationWindowsService.IntegrationDbConnectionString);
        }

        public DataSet GetDataByDataSourceName(string dataSourceName, object commandObj)
        {
            try
            {
                var dataSource = GetDataSource(dataSourceName);

                return dataSource.GetData(commandObj);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public DataSet GetDataByDataSourceId(Guid dataSourceId, object commandObj)
        {
            try
            {
                var dataSource = GetDataSource(dataSourceId);

                return dataSource.GetData(commandObj);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public JobBatch ProcessBatchByDataSourceName(string dataSourceName, SyncSide syncSide, List<EntityBatch> entityBatches)
        {
            try
            {
                var dataSource = GetDataSource(dataSourceName);

                return ProcessJobBatch(syncSide, dataSource, entityBatches);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public JobBatch ProcessBatchByDataSourceId(Guid dataSourceId, SyncSide syncSide, List<EntityBatch> entityBatches)
        {
            try
            {
                var dataSource = GetDataSource(dataSourceId);

                return ProcessJobBatch(syncSide, dataSource, entityBatches);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        private JobBatch ProcessJobBatch(SyncSide syncSide, DataSource dataSource, List<EntityBatch> entityBatches)
        {
            var jobDataSource = new JobDataSource(syncSide, dataSource);

            var jobBatch = new JobBatch(syncSide, jobDataSource);

            foreach (var entityBatch in entityBatches)
            {
                jobBatch.EntityBatches.Add(entityBatch);
            }

            dataSource.ProcessBatch(jobBatch);

            return jobBatch;
        }

        public object ExecuteCommandByDataSourceName(string dataSourceName, object commandObj)
        {
            try
            {
                var dataSource = GetDataSource(dataSourceName);

                return dataSource.ExecuteCommand(commandObj);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public object ExecuteCommandByDataSourceId(Guid dataSourceId, object commandObj)
        {
            try
            {
                var dataSource = GetDataSource(dataSourceId);

                return dataSource.ExecuteCommand(commandObj);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        private DataSource GetDataSource(string dataSourceName)
        {
            var dataSource = configurator.GetDataSourceByName(dataSourceName);

            if (dataSource == null)
                throw new RecordNotFoundException(string.Format("Data source with name '{0}' does not exist.", dataSourceName));

            return dataSource;
        }

        private DataSource GetDataSource(Guid dataSourceId)
        {
            var dataSource = configurator.GetDataSourceById(dataSourceId);

            if (dataSourceId == Guid.Empty)
                throw new Exception("A valid GUID is required for the data source.");

            if (dataSource == null)
                throw new RecordNotFoundException(string.Format("Data source with ID '{0}' does not exist.", dataSourceId));

            return dataSource;
        }
    }
}
