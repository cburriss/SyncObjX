CREATE TABLE [dbo].[JobHistory] (
    [JobHistoryId]       INT              IDENTITY (1, 1) NOT NULL,
    [JobInstanceId]      UNIQUEIDENTIFIER NOT NULL,
    [QueueRequestId]     UNIQUEIDENTIFIER NOT NULL,
    [JobId]              UNIQUEIDENTIFIER NOT NULL,
    [SourceDataSourceId] UNIQUEIDENTIFIER NOT NULL,
    [TargetDataSourceId] UNIQUEIDENTIFIER NOT NULL,
    [Filters]            NVARCHAR (MAX)   NULL,
    [CreatedDate]        DATETIME         NOT NULL,
    [UpdatedDate]        DATETIME         NOT NULL,
    [InvocationSource]   NVARCHAR (50)    NULL,
    [ScheduledStartTime] DATETIME         NOT NULL,
    [ActualStartTime]    DATETIME         NULL,
    [TimeToStartDelay]   TIME (7)         NULL,
    [ActualEndTime]      DATETIME         NULL,
    [ActualDuration]     TIME (7)         NULL,
    [IsOnDemand]         BIT              NOT NULL,
    [JobQueueStatusId]   TINYINT          NOT NULL,
    [HasRecordErrors]    BIT              NOT NULL,
    [HasRuntimeErrors]   BIT              NOT NULL,
    CONSTRAINT [PK_JobHistory] PRIMARY KEY CLUSTERED ([JobHistoryId] ASC),
    CONSTRAINT [FK_JobHistory_DataSource] FOREIGN KEY ([SourceDataSourceId]) REFERENCES [dbo].[DataSource] ([DataSourceId]),
    CONSTRAINT [FK_JobHistory_DataSource1] FOREIGN KEY ([TargetDataSourceId]) REFERENCES [dbo].[DataSource] ([DataSourceId]),
    CONSTRAINT [FK_JobHistory_JobQueueStatus] FOREIGN KEY ([JobQueueStatusId]) REFERENCES [dbo].[JobQueueStatus] ([JobQueueStatusId])
);

























