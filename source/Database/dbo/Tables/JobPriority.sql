CREATE TABLE [dbo].[JobPriority] (
    [JobPriorityId]            TINYINT       NOT NULL,
    [Name]                     NVARCHAR (50) NOT NULL,
    [MaxDelayedStartInSeconds] INT           NULL,
    CONSTRAINT [PK_JobPriority] PRIMARY KEY CLUSTERED ([JobPriorityId] ASC)
);



