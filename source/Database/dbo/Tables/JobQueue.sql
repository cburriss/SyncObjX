CREATE TABLE [dbo].[JobQueue] (
    [JobInstanceId]      UNIQUEIDENTIFIER NOT NULL,
    [QueueRequestId]     UNIQUEIDENTIFIER NOT NULL,
    [JobId]              UNIQUEIDENTIFIER NOT NULL,
    [Filters]            NVARCHAR (MAX)   NULL,
    [CreatedDate]        DATETIME         NOT NULL,
    [UpdatedDate]        DATETIME         NOT NULL,
    [SourceDataSourceId] UNIQUEIDENTIFIER NOT NULL,
    [TargetDataSourceId] UNIQUEIDENTIFIER NOT NULL,
    [InvocationSource]   NVARCHAR (50)    NULL,
    [ScheduledStartTime] DATETIME         NOT NULL,
    [ActualStartTime]    DATETIME         NULL,
    [IsOnDemand]         BIT              NOT NULL,
    [JobQueueStatusId]   TINYINT          NOT NULL,
    CONSTRAINT [PK_JobQueue_1] PRIMARY KEY CLUSTERED ([JobInstanceId] ASC),
    CONSTRAINT [FK_JobQueue_DataSource] FOREIGN KEY ([SourceDataSourceId]) REFERENCES [dbo].[DataSource] ([DataSourceId]),
    CONSTRAINT [FK_JobQueue_DataSource1] FOREIGN KEY ([TargetDataSourceId]) REFERENCES [dbo].[DataSource] ([DataSourceId]),
    CONSTRAINT [FK_JobQueue_Job1] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Job] ([JobId]),
    CONSTRAINT [FK_JobQueue_JobQueueStatus] FOREIGN KEY ([JobQueueStatusId]) REFERENCES [dbo].[JobQueueStatus] ([JobQueueStatusId])
);



















