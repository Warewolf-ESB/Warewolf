CREATE PROCEDURE [Profiler].[usp_GetSpecialHolidays] @FromDate DATE = NULL
	,@ToDate DATE = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF @FromDate IS NULL
		AND @ToDate IS NULL
	BEGIN
		SELECT [Date]
			,[HolidayName]
			,[Description]
		FROM [ProfilerDB3].[Profiler].[SpecialHolidays]
		ORDER BY [Date];
	END
	ELSE
	BEGIN
		SELECT [Date]
			,[HolidayName]
			,[Description]
		FROM [ProfilerDB3].[Profiler].[SpecialHolidays]
		WHERE [Date] BETWEEN @FromDate
				AND @ToDate
		ORDER BY [Date];
	END
END
