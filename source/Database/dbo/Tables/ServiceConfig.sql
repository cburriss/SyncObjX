CREATE TABLE [dbo].[ServiceConfig] (
    [PropertyKey]   VARCHAR (100) NOT NULL,
    [PropertyValue] VARCHAR (MAX) NULL,
    [CreatedDate]   DATETIME      NOT NULL,
    [UpdatedDate]   DATETIME      NOT NULL,
    CONSTRAINT [PK_ServiceConfig] PRIMARY KEY CLUSTERED ([PropertyKey] ASC)
);



