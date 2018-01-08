CREATE TABLE [dbo].[JobSchedule] (
    [JobScheduleId]      UNIQUEIDENTIFIER NOT NULL,
    [JobId]              UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]        DATETIME         NOT NULL,
    [UpdatedDate]        DATETIME         NOT NULL,
    [IsDeleted]          BIT              NOT NULL,
    [IsEnabled]          BIT              NOT NULL,
    [StartDate]          DATE             NOT NULL,
    [EndDate]            DATE             NOT NULL,
    [DaysOfWeek]         NVARCHAR (25)    NOT NULL,
    [StartTime]          TIME (7)         NOT NULL,
    [EndTime]            TIME (7)         NOT NULL,
    [FrequencyInSeconds] INT              NOT NULL,
    CONSTRAINT [PK_JobSchedule_1] PRIMARY KEY CLUSTERED ([JobScheduleId] ASC),
    CONSTRAINT [FK_JobSchedule_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Job] ([JobId])
);











