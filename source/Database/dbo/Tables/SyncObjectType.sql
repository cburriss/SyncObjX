CREATE TABLE [dbo].[SyncObjectType] (
    [SyncObjectTypeId] TINYINT       IDENTITY (1, 1) NOT NULL,
    [Name]             NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_SyncObject] PRIMARY KEY CLUSTERED ([SyncObjectTypeId] ASC)
);

