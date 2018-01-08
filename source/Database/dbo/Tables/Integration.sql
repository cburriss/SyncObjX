CREATE TABLE [dbo].[Integration] (
    [IntegrationId]                UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]                  DATETIME         NOT NULL,
    [UpdatedDate]                  DATETIME         NOT NULL,
    [IsDeleted]                    BIT              NOT NULL,
    [Name]                         NVARCHAR (250)   NOT NULL,
    [Description]                  NVARCHAR (MAX)   NULL,
    [PackageDllDirectory]          NVARCHAR (500)   NOT NULL,
    [PackageDllFilename]           NVARCHAR (250)   NOT NULL,
    [IsEnabled]                    BIT              NOT NULL,
    [SourceName]                   NVARCHAR (100)   NOT NULL,
    [TargetName]                   NVARCHAR (100)   NOT NULL,
    [MaxConcurrentThreads]         INT              NOT NULL,
    [LoggingLevelId]               TINYINT          NOT NULL,
    [LogToDatabase]                BIT              NOT NULL,
    [DaysOfDatabaseLoggingHistory] INT              NOT NULL,
    [LogToFile]                    BIT              NOT NULL,
    [DaysOfFileLoggingHistory]     INT              NOT NULL,
    CONSTRAINT [PK_Integration_1] PRIMARY KEY CLUSTERED ([IntegrationId] ASC),
    CONSTRAINT [FK_Integration_LoggingLevel] FOREIGN KEY ([LoggingLevelId]) REFERENCES [dbo].[LoggingLevel] ([LoggingLevelId])
);









