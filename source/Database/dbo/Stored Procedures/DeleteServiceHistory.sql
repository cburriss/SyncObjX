CREATE PROCEDURE [dbo].[DeleteServiceHistory]
    @DaysToKeep int 
AS 

    SET NOCOUNT ON;
    
    IF @DaysToKeep < 0
    BEGIN
		RETURN
    END
    
    DECLARE @CutOffDate datetime
    
    -- ensures there isn't a datetime overflow
    IF @DaysToKeep > 1000000
    BEGIN
		SET @CutOffDate = CONVERT(datetime, '1/1/2000')
    END
	ELSE
	BEGIN
		SET @CutOffDate = DATEADD(dd, -@DaysToKeep + 1, DATEDIFF(dd, 0, GETDATE()))
	END
    
    -- Log Entry
	DELETE FROM LogEntry
	WHERE IntegrationId IS NULL AND CreatedDate < @CutOffDate