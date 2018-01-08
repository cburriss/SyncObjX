CREATE TABLE [dbo].[JobTerminateOnErrorType] (
    [JobTerminateOnErrorTypeId] TINYINT        NOT NULL,
    [Name]                      NVARCHAR (50)  NOT NULL,
    [Description]               NVARCHAR (250) NOT NULL,
    CONSTRAINT [PK_JobTerminateOnErrorType] PRIMARY KEY CLUSTERED ([JobTerminateOnErrorTypeId] ASC)
);

