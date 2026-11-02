CREATE PROCEDURE [Profiler].[usp_UpdateSpecialHoliday] @Date DATE
	,@HolidayName VARCHAR(50)
	,@Description VARCHAR(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [Profiler].[SpecialHolidays]
	SET [HolidayName] = @HolidayName
		,[Description] = @Description
	WHERE [Date] = @Date
END
