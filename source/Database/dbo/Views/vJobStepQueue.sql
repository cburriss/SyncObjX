
CREATE VIEW [dbo].[vJobStepQueue]
AS
SELECT jq.IntegrationName, jq.JobName,  js.Name AS JobStepName, jsq.OrderIndex, 
jsq.JobInstanceId, jsq.JobStepInstanceId, 
jsq.ActualStartTime, jsq.ActualEndTime, 
jsqs.Name AS JobStepStatus, jsq.CreatedDate, jsq.UpdatedDate
FROM JobStepQueue jsq
LEFT JOIN JobStep js ON js.JobStepId = jsq.JobStepId
LEFT JOIN vJobQueue jq ON jq.JobInstanceId = jsq.JobInstanceId
LEFT JOIN JobStepQueueStatus jsqs ON jsqs.JobStepQueueStatusId = jsq.JobStepQueueStatusId