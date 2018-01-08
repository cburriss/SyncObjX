CREATE TABLE [dbo].[LogEntry] (
    [LogEntryId]        INT              IDENTITY (1, 1) NOT NULL,
    [IntegrationId]     UNIQUEIDENTIFIER NULL,
    [DataSourceId]      UNIQUEIDENTIFIER NULL,
    [JobInstanceId]     UNIQUEIDENTIFIER NULL,
    [JobStepInstanceId] UNIQUEIDENTIFIER NULL,
    [LogEntryTypeId]    TINYINT          NOT NULL,
    [CreatedDate]       DATETIME         NOT NULL,
    [Message]           NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_JobHistoryError] PRIMARY KEY CLUSTERED ([LogEntryId] ASC),
    CONSTRAINT [FK_LogEntry_LogEntryType] FOREIGN KEY ([LogEntryTypeId]) REFERENCES [dbo].[LogEntryType] ([LogEntryTypeId])
);



















