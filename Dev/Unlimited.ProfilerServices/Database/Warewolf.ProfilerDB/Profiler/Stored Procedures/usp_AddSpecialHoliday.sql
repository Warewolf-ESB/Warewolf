CREATE PROCEDURE [Profiler].[usp_AddSpecialHoliday] @Date DATE
	,@HolidayName VARCHAR(50)
	,@Description VARCHAR(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [Profiler].[SpecialHolidays] (
		[Date]
		,[HolidayName]
		,[Description]
		)
	VALUES (
		@Date
		,@HolidayName
		,@Description
		)
END
