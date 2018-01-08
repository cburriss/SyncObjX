CREATE TABLE [dbo].[JobDataSource] (
    [JobDataSourceId] UNIQUEIDENTIFIER NOT NULL,
    [JobId]           UNIQUEIDENTIFIER NOT NULL,
    [DataSourceId]    UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]     DATETIME         NOT NULL,
    [UpdatedDate]     DATETIME         NOT NULL,
    [IsDeleted]       BIT              NOT NULL,
    [SyncSide]        CHAR (6)         NOT NULL,
    CONSTRAINT [PK_JobDataSource] PRIMARY KEY CLUSTERED ([JobDataSourceId] ASC),
    CONSTRAINT [FK_JobDataSource_DataSource] FOREIGN KEY ([DataSourceId]) REFERENCES [dbo].[DataSource] ([DataSourceId]),
    CONSTRAINT [FK_JobDataSource_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Job] ([JobId])
);

















