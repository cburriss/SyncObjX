CREATE TABLE [dbo].[ExtendedProperty] (
    [ExtendedPropertyId] INT              IDENTITY (1, 1) NOT NULL,
    [SyncObjectTypeId]   TINYINT          NOT NULL,
    [SyncObjectId]       UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]        DATETIME         NOT NULL,
    [UpdatedDate]        DATETIME         NOT NULL,
    [IsDeleted]          BIT              NOT NULL,
    [Key]                NVARCHAR (250)   NOT NULL,
    [Value]              NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_ExtendedProperty] PRIMARY KEY CLUSTERED ([ExtendedPropertyId] ASC),
    CONSTRAINT [FK_ExtendedProperty_SyncObjectType] FOREIGN KEY ([SyncObjectTypeId]) REFERENCES [dbo].[SyncObjectType] ([SyncObjectTypeId])
);





