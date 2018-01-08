
CREATE PROCEDURE [dbo].[UpdateJobDataSourceHistory]
	@JobId uniqueidentifier,	@SourceDataSourceId uniqueidentifier,	@TargetDataSourceId uniqueidentifier,	@QueueRequestId uniqueidentifier,	@JobInstanceId uniqueidentifier,	@ActualStartTime datetime,	@ActualEndTime datetime,
	@HasRecordErrors bit,
	@HasRuntimeErrors bit
AS
BEGIN
	SET NOCOUNT ON;

    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    
	BEGIN TRAN
	
	IF EXISTS (SELECT 1 
			   FROM JobDataSourceHistory (UPDLOCK)
			   WHERE JobId = @JobId 
			   AND SourceDataSourceId = @SourceDataSourceId
			   AND TargetDataSourceId = @TargetDataSourceId)
	BEGIN
	
		UPDATE JobDataSourceHistory 
		SET UpdatedDate = GETDATE(),
		LastQueueRequestId = @QueueRequestId,
		LastJobInstanceId = @JobInstanceId,
		LastStartTime = @ActualStartTime,
		LastEndTime = @ActualEndTime
		WHERE JobId = @JobId
		AND SourceDataSourceId = @SourceDataSourceId
		AND TargetDataSourceId = @TargetDataSourceId

	END
	ELSE  
		INSERT INTO JobDataSourceHistory 
		(JobId, SourceDataSourceId, TargetDataSourceId, CreatedDate, UpdatedDate,
		 LastQueueRequestId, LastJobInstanceId, LastStartTime, LastEndTime)
		VALUES 
		(@JobId, @SourceDataSourceId, @TargetDataSourceId, GETDATE(), GETDATE(),
		 @QueueRequestId, @JobInstanceId, @ActualStartTime, @ActualEndTime)
	END
	
	IF @HasRecordErrors = 0 AND @HasRuntimeErrors = 0
	BEGIN
		UPDATE JobDataSourceHistory 
		SET UpdatedDate = GETDATE(),
		LastStartTimeWithoutRecordErrors = @ActualStartTime,
		LastEndTimeWithoutRecordErrors = @ActualEndTime
		WHERE JobId = @JobId 
		AND SourceDataSourceId = @SourceDataSourceId
		AND TargetDataSourceId = @TargetDataSourceId
	END
	
	IF @HasRuntimeErrors = 0
	BEGIN
		UPDATE JobDataSourceHistory 
		SET UpdatedDate = GETDATE(),
		LastStartTimeWithoutRuntimeErrors = @ActualStartTime,
		LastEndTimeWithoutRuntimeErrors = @ActualEndTime
		WHERE JobId = @JobId 
		AND SourceDataSourceId = @SourceDataSourceId
		AND TargetDataSourceId = @TargetDataSourceId
	END
	
	COMMIT TRAN