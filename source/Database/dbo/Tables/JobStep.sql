CREATE TABLE [dbo].[JobStep] (
    [JobStepId]          UNIQUEIDENTIFIER NOT NULL,
    [JobId]              UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]        DATETIME         NOT NULL,
    [UpdatedDate]        DATETIME         NOT NULL,
    [IsDeleted]          BIT              NOT NULL,
    [Name]               NVARCHAR (250)   NOT NULL,
    [Description]        NVARCHAR (MAX)   NULL,
    [FullyQualifiedName] NVARCHAR (250)   NOT NULL,
    [OrderIndex]         TINYINT          NOT NULL,
    [IsEnabled]          BIT              NOT NULL,
    CONSTRAINT [PK_JobStep_1] PRIMARY KEY CLUSTERED ([JobStepId] ASC),
    CONSTRAINT [FK_JobStep_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Job] ([JobId])
);













