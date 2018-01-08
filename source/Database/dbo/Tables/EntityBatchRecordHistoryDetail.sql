CREATE TABLE [dbo].[EntityBatchRecordHistoryDetail] (
    [EntityBatchRecordHistoryDetailId] INT            IDENTITY (1, 1) NOT NULL,
    [EntityBatchRecordHistoryId]       INT            NOT NULL,
    [CreatedDate]                      DATETIME       NOT NULL,
    [FieldName]                        NVARCHAR (250) NOT NULL,
    [FieldCaption]                     NVARCHAR (500) NULL,
    [OldValue]                         NVARCHAR (MAX) NULL,
    [NewValue]                         NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_EntityBatchHistoryDetail] PRIMARY KEY CLUSTERED ([EntityBatchRecordHistoryDetailId] ASC),
    CONSTRAINT [FK_EntityBatchRecordHistoryDetail_EntityBatchRecordHistory] FOREIGN KEY ([EntityBatchRecordHistoryId]) REFERENCES [dbo].[EntityBatchRecordHistory] ([EntityBatchRecordHistoryId])
);





