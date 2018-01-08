CREATE TABLE [dbo].[JobDataSourceHistory] (
    [JobId]                             UNIQUEIDENTIFIER NOT NULL,
    [SourceDataSourceId]                UNIQUEIDENTIFIER NOT NULL,
    [TargetDataSourceId]                UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]                       DATETIME         NOT NULL,
    [UpdatedDate]                       DATETIME         NOT NULL,
    [LastQueueRequestId]                UNIQUEIDENTIFIER NOT NULL,
    [LastJobInstanceId]                 UNIQUEIDENTIFIER NOT NULL,
    [LastStartTime]                     DATETIME         NOT NULL,
    [LastEndTime]                       DATETIME         NOT NULL,
    [LastStartTimeWithoutRecordErrors]  DATETIME         NULL,
    [LastEndTimeWithoutRecordErrors]    DATETIME         NULL,
    [LastStartTimeWithoutRuntimeErrors] DATETIME         NULL,
    [LastEndTimeWithoutRuntimeErrors]   DATETIME         NULL,
    CONSTRAINT [PK_DataSourceUpdateHistory] PRIMARY KEY CLUSTERED ([JobId] ASC, [SourceDataSourceId] ASC, [TargetDataSourceId] ASC)
);





