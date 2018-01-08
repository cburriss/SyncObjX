CREATE TABLE [dbo].[JobStepHistory] (
    [JobStepHistoryId]     INT              IDENTITY (1, 1) NOT NULL,
    [JobStepInstanceId]    UNIQUEIDENTIFIER NOT NULL,
    [JobInstanceId]        UNIQUEIDENTIFIER NOT NULL,
    [JobHistoryId]         INT              NOT NULL,
    [JobStepId]            UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]          DATETIME         NOT NULL,
    [UpdatedDate]          DATETIME         NOT NULL,
    [OrderIndex]           TINYINT          NOT NULL,
    [ActualStartTime]      DATETIME         NULL,
    [ActualEndTime]        DATETIME         NULL,
    [ActualDuration]       TIME (7)         NULL,
    [JobStepQueueStatusId] TINYINT          NOT NULL,
    [HasRecordErrors]      BIT              NOT NULL,
    [HasRuntimeErrors]     BIT              NOT NULL,
    CONSTRAINT [PK_JobStepHistory] PRIMARY KEY CLUSTERED ([JobStepHistoryId] ASC),
    CONSTRAINT [FK_JobStepHistory_JobHistory] FOREIGN KEY ([JobHistoryId]) REFERENCES [dbo].[JobHistory] ([JobHistoryId]),
    CONSTRAINT [FK_JobStepHistory_JobStep] FOREIGN KEY ([JobStepId]) REFERENCES [dbo].[JobStep] ([JobStepId]),
    CONSTRAINT [FK_JobStepHistory_JobStepQueueStatus] FOREIGN KEY ([JobStepQueueStatusId]) REFERENCES [dbo].[JobStepQueueStatus] ([JobStepQueueStatusId])
);



















