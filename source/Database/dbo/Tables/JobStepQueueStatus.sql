CREATE TABLE [dbo].[JobStepQueueStatus] (
    [JobStepQueueStatusId] TINYINT        NOT NULL,
    [Name]                 NVARCHAR (50)  NOT NULL,
    [Description]          NVARCHAR (250) NOT NULL,
    CONSTRAINT [PK_JobStepStatus] PRIMARY KEY CLUSTERED ([JobStepQueueStatusId] ASC)
);

