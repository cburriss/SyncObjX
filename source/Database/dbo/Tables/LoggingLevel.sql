CREATE TABLE [dbo].[LoggingLevel] (
    [LoggingLevelId] TINYINT        NOT NULL,
    [Description]    NVARCHAR (250) NULL,
    CONSTRAINT [PK_LoggingLevel] PRIMARY KEY CLUSTERED ([LoggingLevelId] ASC)
);

