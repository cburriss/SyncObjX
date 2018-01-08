CREATE PROCEDURE [dbo].[DeleteIntegrationHistory]
    @IntegrationId uniqueidentifier, 
    @DaysToKeep int 
AS 

    SET NOCOUNT ON;
    
    IF @DaysToKeep < 0
    BEGIN
		RETURN
    END
    
    DECLARE @CutOffDate datetime
    
    -- ensures there isn't a datetime overflow
    IF @DaysToKeep > 1000000
    BEGIN
		SET @CutOffDate = CONVERT(datetime, '1/1/2000')
    END
	ELSE
	BEGIN
		SET @CutOffDate = DATEADD(dd, -@DaysToKeep + 1, DATEDIFF(dd, 0, GETDATE()))
	END
    
    -- Job Step History
    DELETE jsh
	FROM JobStepHistory jsh
	INNER JOIN JobHistory jh ON jh.JobHistoryId = jsh.JobHistoryId
	INNER JOIN Job j ON j.JobId = jh.JobId
	WHERE j.IntegrationId = @IntegrationId AND jh.UpdatedDate < @CutOffDate
	
	-- Job Data Source History
	IF @DaysToKeep = 0
	BEGIN
		DELETE jdsh
		FROM JobDataSourceHistory jdsh
		INNER JOIN Job j ON j.JobId = jdsh.JobId
		WHERE j.IntegrationId = @IntegrationId
	END

	-- Job History
	DELETE jh
	FROM JobHistory jh
	INNER JOIN Job j ON j.JobId = jh.JobId
	WHERE j.IntegrationId = @IntegrationId AND jh.UpdatedDate < @CutOffDate
	
	-- Entity Batch Record History Detail
	DELETE rhd
	FROM EntityBatchRecordHistoryDetail rhd
	INNER JOIN EntityBatchRecordHistory rh ON rh.EntityBatchRecordHistoryId = rhd.EntityBatchRecordHistoryId
	INNER JOIN EntityBatchHistory h ON h.EntityBatchHistoryId = rh.EntityBatchHistoryId
	WHERE h.IntegrationId = @IntegrationId AND rhd.CreatedDate < @CutOffDate
	
	-- Entity Batch Record History
	DELETE rh
	FROM EntityBatchRecordHistory rh
	INNER JOIN EntityBatchHistory h ON h.EntityBatchHistoryId = rh.EntityBatchHistoryId
	WHERE h.IntegrationId = @IntegrationId AND rh.CreatedDate < @CutOffDate
	
	-- Entity Batch
	DELETE FROM EntityBatchHistory
	WHERE IntegrationId = @IntegrationId AND CreatedDate < @CutOffDate
	
	-- Log Entry
	DELETE FROM LogEntry
	WHERE IntegrationId = @IntegrationId AND CreatedDate < @CutOffDate