CREATE TABLE [dbo].[LogEntryType] (
    [LogEntryTypeId] TINYINT       NOT NULL,
    [Name]           NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_LogEntryType] PRIMARY KEY CLUSTERED ([LogEntryTypeId] ASC)
);

