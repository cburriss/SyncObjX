CREATE PROCEDURE [dbo].[DeleteJobDataSourceHistory]
    @JobId uniqueidentifier
AS 

	DELETE jdsh
	FROM JobDataSourceHistory jdsh
	WHERE jdsh.JobId = @JobId