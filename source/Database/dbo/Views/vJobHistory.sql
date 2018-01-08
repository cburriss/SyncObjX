CREATE VIEW vJobHistory
AS
SELECT i.IntegrationId, i.Name AS IntegrationName, j.Name AS JobName, sds.Name AS SourceDataSource, tds.Name AS TargetDataSource, jh.JobInstanceId, jh.QueueRequestId, 
Filters, InvocationSource,
ScheduledStartTime, ActualStartTime, jh.TimeToStartDelay, jh.ActualEndTime, jh.ActualDuration,
jh.IsOnDemand, jqs.Name AS [Status], jh.HasRecordErrors, jh.HasRuntimeErrors
FROM JobHistory jh
INNER JOIN Job j ON j.JobId = jh.JobId
INNER JOIN Integration i ON i.IntegrationId = j.IntegrationId
INNER JOIN JobQueueStatus jqs ON jqs.JobQueueStatusId = jh.JobQueueStatusId
INNER JOIN DataSource sds ON sds.DataSourceId = jh.SourceDataSourceId
INNER JOIN DataSource tds ON tds.DataSourceId = jh.TargetDataSourceId