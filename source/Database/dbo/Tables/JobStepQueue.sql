CREATE TABLE [dbo].[JobStepQueue] (
    [JobStepInstanceId]    UNIQUEIDENTIFIER NOT NULL,
    [JobInstanceId]        UNIQUEIDENTIFIER NOT NULL,
    [JobStepId]            UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]          DATETIME         NOT NULL,
    [UpdatedDate]          DATETIME         NOT NULL,
    [OrderIndex]           TINYINT          NOT NULL,
    [ActualStartTime]      DATETIME         NULL,
    [ActualEndTime]        DATETIME         NULL,
    [JobStepQueueStatusId] TINYINT          NOT NULL,
    CONSTRAINT [PK_JobStepQueue_1] PRIMARY KEY CLUSTERED ([JobStepInstanceId] ASC),
    CONSTRAINT [FK_JobStepQueue_JobQueue] FOREIGN KEY ([JobInstanceId]) REFERENCES [dbo].[JobQueue] ([JobInstanceId]),
    CONSTRAINT [FK_JobStepQueue_JobStep] FOREIGN KEY ([JobStepId]) REFERENCES [dbo].[JobStep] ([JobStepId]),
    CONSTRAINT [FK_JobStepQueue_JobStepQueueStatus] FOREIGN KEY ([JobStepQueueStatusId]) REFERENCES [dbo].[JobStepQueueStatus] ([JobStepQueueStatusId])
);















