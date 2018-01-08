﻿CREATE PROC [dbo].[AddLogEntry]	@IntegrationId uniqueidentifier,	@DataSourceId uniqueidentifier,	@JobInstanceId uniqueidentifier,	@JobStepInstanceId uniqueidentifier,	@LogEntryTypeId tinyint,	@Message nvarchar(MAX)AS	SET NOCOUNT ON	INSERT INTO LogEntry (		[IntegrationId],		[DataSourceId],		[JobInstanceId],		[JobStepInstanceId],		[LogEntryTypeId],		[CreatedDate],		[Message]	)	VALUES (		@IntegrationId,		@DataSourceId,		@JobInstanceId,		@JobStepInstanceId,		@LogEntryTypeId,		GETDATE(),		@Message	)