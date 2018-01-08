CREATE TABLE [dbo].[Job] (
    [JobId]                  UNIQUEIDENTIFIER NOT NULL,
    [IntegrationId]          UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]            DATETIME         NOT NULL,
    [UpdatedDate]            DATETIME         NOT NULL,
    [IsDeleted]              BIT              NOT NULL,
    [Name]                   NVARCHAR (250)   NOT NULL,
    [Description]            NVARCHAR (MAX)   NULL,
    [IsEnabled]              BIT              NOT NULL,
    [JobPriorityId]          TINYINT          NOT NULL,
    [TerminateOnErrorTypeId] TINYINT          NOT NULL,
    [LoggingLevelId]         TINYINT          NOT NULL,
    CONSTRAINT [PK_Job_1] PRIMARY KEY CLUSTERED ([JobId] ASC),
    CONSTRAINT [FK_Job_Integration] FOREIGN KEY ([IntegrationId]) REFERENCES [dbo].[Integration] ([IntegrationId]),
    CONSTRAINT [FK_Job_JobPriority] FOREIGN KEY ([JobPriorityId]) REFERENCES [dbo].[JobPriority] ([JobPriorityId]),
    CONSTRAINT [FK_Job_JobTerminateOnErrorType] FOREIGN KEY ([TerminateOnErrorTypeId]) REFERENCES [dbo].[JobTerminateOnErrorType] ([JobTerminateOnErrorTypeId]),
    CONSTRAINT [FK_Job_LoggingLevel] FOREIGN KEY ([LoggingLevelId]) REFERENCES [dbo].[LoggingLevel] ([LoggingLevelId])
);











