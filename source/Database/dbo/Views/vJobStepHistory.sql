CREATE VIEW vJobStepHistory
AS
SELECT i.IntegrationId, i.Name AS IntegrationName, j.Name AS JobName, js.Name AS JobStepName, js.OrderIndex AS JobStepOrderIndex, sds.Name AS SourceDataSource, tds.Name AS TargetDataSource, jsh.JobInstanceId, jh.QueueRequestId, 
jsh.ActualStartTime, jsh.ActualEndTime, jsh.ActualDuration,
jqs.Name AS [Status], jsh.HasRecordErrors, jsh.HasRuntimeErrors
FROM JobStepHistory jsh
INNER JOIN JobHistory jh ON jh.JobHistoryId = jsh.JobHistoryId
INNER JOIN JobStep js ON js.JobId = jh.JobId
INNER JOIN Job j ON j.JobId = jh.JobId
INNER JOIN Integration i ON i.IntegrationId = j.IntegrationId
INNER JOIN JobQueueStatus jqs ON jqs.JobQueueStatusId = jh.JobQueueStatusId
INNER JOIN DataSource sds ON sds.DataSourceId = jh.SourceDataSourceId
INNER JOIN DataSource tds ON tds.DataSourceId = jh.TargetDataSourceId