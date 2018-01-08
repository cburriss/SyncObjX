CREATE PROCEDURE [dbo].[DeleteAllHistory]
AS 

    SET NOCOUNT ON;
       
    -- Job Step History
    DELETE FROM JobStepHistory

	-- Job History
	DELETE FROM JobHistory
	
	-- Job Data Source History
    DELETE FROM JobDataSourceHistory
	
	-- Entity Batch Record History Detail
	DELETE FROM EntityBatchRecordHistoryDetail
	
	-- Entity Batch Record History
	DELETE FROM EntityBatchRecordHistory
	
	-- Entity Batch
	DELETE FROM EntityBatchHistory
	
	-- Log Entry
	DELETE FROM LogEntry