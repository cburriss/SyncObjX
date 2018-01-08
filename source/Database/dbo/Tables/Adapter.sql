CREATE TABLE [dbo].[Adapter] (
    [AdapterId]          UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]        DATETIME         NOT NULL,
    [UpdatedDate]        DATETIME         NOT NULL,
    [IsDeleted]          BIT              NOT NULL,
    [Name]               NVARCHAR (250)   NOT NULL,
    [Description]        NVARCHAR (MAX)   NULL,
    [DllDirectory]       NVARCHAR (500)   NOT NULL,
    [DllFilename]        NVARCHAR (250)   NOT NULL,
    [FullyQualifiedName] NVARCHAR (250)   NOT NULL,
    CONSTRAINT [PK_Adapter_1] PRIMARY KEY CLUSTERED ([AdapterId] ASC)
);















