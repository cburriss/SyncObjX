using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SyncObjX.Core;
using SyncObjX.Data;
using SyncObjX.Management;
using SyncObjX.SyncObjects;
using SyncObjX.Util;
using ls = SyncObjX.Logging.LinqToSql;

namespace SyncObjX.Logging
{
    public class DatabaseLogger : ISyncEngineLogger
    {
        public readonly string ConnectionString;

        public DatabaseLogger(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new Exception("Connection string is null or empty.");

            // test connection to configuration database; if a failure occurs, an exception will be thrown
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            conn.Close();

            ConnectionString = connectionString;
        }

        public void DeleteJobDataSourceHistory(Guid jobId)
        {
            using (var dbContext = new ls.LoggingDataContext(ConnectionString))
            {
                try
                {
                    dbContext.CommandTimeout = 60 * 15; // 15 minutes
                    dbContext.DeleteJobDataSourceHistory(jobId);
                }
                catch (Exception ex)
                {
                    SyncEngineLogger.WriteExceptionToLog(ex);
                }
            }
        }

        public void DeleteIntegrationHistory(Guid integrationId, int daysToKeep)
        {
            using (var dbContext = new ls.LoggingDataContext(ConnectionString))
            {
                try
                {
                    dbContext.CommandTimeout = 60 * 60; // 60 minutes
                    dbContext.DeleteIntegrationHistory(integrationId, daysToKeep);
                }
                catch (Exception ex)
                {
                    SyncEngineLogger.WriteExceptionToLog(ex);
                }
            }
        }

        public void DeleteServiceHistory(int daysToKeep)
        {
            using (var dbContext = new ls.LoggingDataContext(ConnectionString))
            {
                try
                {
                    dbContext.CommandTimeout = 60 * 60; // 60 minutes
                    dbContext.DeleteServiceHistory(daysToKeep);
                }
                catch (Exception ex)
                {
                    SyncEngineLogger.WriteExceptionToLog(ex);
                }
            }
        }

        public void DeleteAllHistory()
        {
            using (var dbContext = new ls.LoggingDataContext(ConnectionString))
            {
                try
                {
                    dbContext.CommandTimeout = 60 * 60; // 60 minutes
                    dbContext.DeleteAllHistory();
                }
                catch (Exception ex)
                {
                    SyncEngineLogger.WriteExceptionToLog(ex);
                }
            }
        }

        public void WriteToLog(LogEntryType logEntryType, Integration integration, DataSource dataSource, JobInstance jobInstance, JobStepInstance jobStepInstance, string message)
        {
            using (var dbContext = new ls.LoggingDataContext(ConnectionString))
            {
                Guid? integrationId = null;
                Guid? dataSourceId = null;
                Guid? jobInstanceId = null;
                Guid? jobStepInstanceId = null;

                if (integration != null)
                    integrationId = integration.Id;

                if (dataSource != null)
                    dataSourceId = dataSource.Id;

                if (jobInstance != null)
                    jobInstanceId = jobInstance.Id;

                if (jobStepInstance != null)
                    jobStepInstanceId = jobStepInstance.Id;

                dbContext.AddLogEntry(integrationId, dataSourceId, jobInstanceId, jobStepInstanceId, (byte)logEntryType, message);
            }
        }

        public void WriteToLog(JobInstance jobInstance, JobStepInstance jobStepInstance, JobBatch jobBatch)
        {
            using (var dbContext = new ls.LoggingDataContext(ConnectionString))
            {
                foreach (var entityBatch in jobBatch.EntityBatches)
                {
                    int countOfInsertsWithoutError = 0;
                    int countOfUpdatesWithoutError = 0;
                    int countOfDeletionsWithoutError = 0;

                    if (!entityBatch.HasBeenProcessed)
                    {
                        WriteToLog(LogEntryType.Error, jobInstance.Integration, jobBatch.AssociatedDataSource.DataSource, jobInstance, jobStepInstance, "The entity batch has not been processed by a data source. No history can be logged.");
                        continue;
                    }

                    var entityBatchHistoryId = (int)dbContext.AddEntityBatchHistory(jobInstance.Integration.Id, jobInstance.Id, jobStepInstance.Id,
                                                       jobBatch.AssociatedDataSource.DataSource.Id, Enum.GetName(typeof(SyncSide), jobBatch.SyncSide),
                                                       entityBatch.EntityDefinition.TechnicalEntityName, entityBatch.EntityDefinition.UserFriendlyEntityName,
                                                       StringHelper.GetDelimitedString(entityBatch.EntityDefinition.PrimaryKeyColumnNames))
                                                       .FirstOrDefault().InsertedID;


                    string batchHistoryTemplate = @"DECLARE @EntityBatchRecordId int
                                                    INSERT INTO EntityBatchRecordHistory VALUES ({0}, GETDATE(), '{1}', '{2}', '{3}', {4})
                                                    SELECT @EntityBatchRecordId = SCOPE_IDENTITY()";

                    string batchHistoryDetailTemplate = "\n\rINSERT INTO EntityBatchRecordHistoryDetail VALUES (@EntityBatchRecordId, GETDATE(), '{0}', NULL, {1}, {2})";
                    
                    // log inserts
                    foreach (var insertedRecord in entityBatch.RecordsToAdd)
                    {
                        //var entityBatchHistoryRecordId = (int)dbContext.AddEntityBatchRecordHistory(entityBatchHistoryId, 'I', StringHelper.GetDelimitedStringFromList(insertedRecord.PrimaryKeyValues),
                        //                                        insertedRecord.HasError, insertedRecord.ErrorMessage).FirstOrDefault().InsertedID;

                        if (entityBatch.LoggingBehavior.MaxNumberOfInsertsWithoutErrorToLog.HasValue && !insertedRecord.HasError)
                        {
                            countOfInsertsWithoutError++;

                            if (countOfInsertsWithoutError > entityBatch.LoggingBehavior.MaxNumberOfInsertsWithoutErrorToLog.Value)
                                continue;
                        }

                        StringBuilder sqlForInsert = new StringBuilder();

                        sqlForInsert.AppendFormat(batchHistoryTemplate,
                            entityBatchHistoryId, "I", StringHelper.GetDelimitedString(insertedRecord.PrimaryKeyValues), insertedRecord.HasError, 
                            insertedRecord.ErrorMessage == null ? "NULL" : "'" + insertedRecord.ErrorMessage.Replace("'", "''") + "'");
                        
                        foreach (var fieldValuePair in insertedRecord.FieldValuePairs)
                        {
                            //dbContext.AddEntityBatchRecordHistoryDetail(entityBatchHistoryRecordId, fieldValuePair.Key, null, null, fieldValuePair.Value);

                            sqlForInsert.AppendFormat(batchHistoryDetailTemplate,
                                fieldValuePair.Key, "NULL", fieldValuePair.Value == null ? "NULL" : "'" + fieldValuePair.Value.Replace("'", "''") + "'");
                        }

                        dbContext.ExecuteCommand(sqlForInsert.ToString());
                    }

                    // log updates
                    foreach (var updatedRecord in entityBatch.RecordsToUpdate)
                    {
                        //var entityBatchHistoryRecordId = (int)dbContext.AddEntityBatchRecordHistory(entityBatchHistoryId, 'U', StringHelper.GetDelimitedStringFromList(updatedRecord.PrimaryKeyValues),
                        //                                        updatedRecord.HasError, updatedRecord.ErrorMessage).FirstOrDefault().InsertedID;

                        if (entityBatch.LoggingBehavior.MaxNumberOfUpdatesWithoutErrorToLog.HasValue && !updatedRecord.HasError)
                        {
                            countOfUpdatesWithoutError++;

                            if (countOfUpdatesWithoutError > entityBatch.LoggingBehavior.MaxNumberOfUpdatesWithoutErrorToLog.Value)
                                continue;
                        }

                        StringBuilder sqlForUpdate = new StringBuilder();

                        sqlForUpdate.AppendFormat(batchHistoryTemplate,
                            entityBatchHistoryId, "U", StringHelper.GetDelimitedString(updatedRecord.PrimaryKeyValues), updatedRecord.HasError,
                            updatedRecord.ErrorMessage == null ? "NULL" : "'" + updatedRecord.ErrorMessage.Replace("'", "''") + "'");

                        foreach (var fieldValuePair in updatedRecord.FieldValuePairs)
                        {
                            //dbContext.AddEntityBatchRecordHistoryDetail(entityBatchHistoryRecordId, fieldValuePair.Key, null, fieldValuePair.Value.OldValue, fieldValuePair.Value.NewValue);

                            sqlForUpdate.AppendFormat(batchHistoryDetailTemplate, 
                                fieldValuePair.Key,
                                fieldValuePair.Value.OldValue == null ? "NULL" : "'" + fieldValuePair.Value.OldValue.Replace("'", "''") + "'",
                                fieldValuePair.Value.NewValue == null ? "NULL" : "'" + fieldValuePair.Value.NewValue.Replace("'", "''") + "'");
                        }

                        dbContext.ExecuteCommand(sqlForUpdate.ToString());
                    }

                    // log deletions
                    foreach (var deletedRecord in entityBatch.RecordsToDelete)
                    {
                        if (entityBatch.LoggingBehavior.MaxNumberOfDeletionsWithoutErrorToLog.HasValue && !deletedRecord.HasError)
                        {
                            countOfDeletionsWithoutError++;

                            if (countOfDeletionsWithoutError > entityBatch.LoggingBehavior.MaxNumberOfDeletionsWithoutErrorToLog.Value)
                                continue;
                        }

                        var entityBatchHistoryRecordId = (int)dbContext.AddEntityBatchRecordHistory(entityBatchHistoryId, 'D', 
                                                                StringHelper.GetDelimitedString(deletedRecord.PrimaryKeyValues),
                                                                deletedRecord.HasError, 
                                                                deletedRecord.ErrorMessage).FirstOrDefault().InsertedID;
                    }
                }
            }
        }
    }
}
