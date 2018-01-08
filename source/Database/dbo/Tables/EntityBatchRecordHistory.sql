CREATE TABLE [dbo].[EntityBatchRecordHistory] (
    [EntityBatchRecordHistoryId] INT            IDENTITY (1, 1) NOT NULL,
    [EntityBatchHistoryId]       INT            NOT NULL,
    [CreatedDate]                DATETIME       NOT NULL,
    [Action]                     CHAR (1)       NOT NULL,
    [EntityKeyValues]            NVARCHAR (250) NOT NULL,
    [HasError]                   BIT            NOT NULL,
    [ErrorMessage]               NTEXT          NULL,
    CONSTRAINT [PK_EntityBatchUpdatesHistory] PRIMARY KEY CLUSTERED ([EntityBatchRecordHistoryId] ASC),
    CONSTRAINT [FK_EntityBatchRecordHistory_EntityBatchHistory] FOREIGN KEY ([EntityBatchHistoryId]) REFERENCES [dbo].[EntityBatchHistory] ([EntityBatchHistoryId])
);









