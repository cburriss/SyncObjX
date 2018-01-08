CREATE TABLE [dbo].[JobQueueStatus] (
    [JobQueueStatusId] TINYINT        IDENTITY (1, 1) NOT NULL,
    [Name]             NVARCHAR (50)  NOT NULL,
    [Description]      NVARCHAR (250) NOT NULL,
    CONSTRAINT [PK_JobQueueStatus] PRIMARY KEY CLUSTERED ([JobQueueStatusId] ASC)
);

