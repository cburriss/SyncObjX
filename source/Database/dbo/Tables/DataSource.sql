CREATE TABLE [dbo].[DataSource] (
    [DataSourceId] UNIQUEIDENTIFIER NOT NULL,
    [AdapterId]    UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]  DATETIME         NOT NULL,
    [UpdatedDate]  DATETIME         NOT NULL,
    [IsDeleted]    BIT              NOT NULL,
    [Name]         NVARCHAR (250)   NOT NULL,
    [Description]  NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_DataSource_1] PRIMARY KEY CLUSTERED ([DataSourceId] ASC),
    CONSTRAINT [FK_DataSource_Adapter] FOREIGN KEY ([AdapterId]) REFERENCES [dbo].[Adapter] ([AdapterId])
);

















