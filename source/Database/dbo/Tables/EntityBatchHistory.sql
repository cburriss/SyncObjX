CREATE TABLE [dbo].[EntityBatchHistory] (
    [EntityBatchHistoryId]   INT              IDENTITY (1, 1) NOT NULL,
    [IntegrationId]          UNIQUEIDENTIFIER NOT NULL,
    [JobInstanceId]          UNIQUEIDENTIFIER NOT NULL,
    [JobStepInstanceId]      UNIQUEIDENTIFIER NOT NULL,
    [DataSourceId]           UNIQUEIDENTIFIER NOT NULL,
    [SyncSide]               CHAR (6)         NOT NULL,
    [CreatedDate]            DATETIME         NOT NULL,
    [TechnicalEntityName]    NVARCHAR (100)   NOT NULL,
    [UserFriendlyEntityName] NVARCHAR (100)   NOT NULL,
    [EntityKeyFields]        NVARCHAR (100)   NOT NULL,
    CONSTRAINT [PK_JobStepHistoryDetail] PRIMARY KEY CLUSTERED ([EntityBatchHistoryId] ASC)
);

























