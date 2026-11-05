CREATE PROCEDURE [Profiler].[usp_GetPublicHolidaysInDateRange] @FromDate AS DATETIME
	,@ToDate AS DATETIME
AS
BEGIN
	IF ISNULL(@FromDate, '') <> ''
		AND ISNULL(@ToDate, '') <> ''
		SELECT 0 AS [HolidayId]
			,[PublicHolidayDate] AS [Date]
			,PublicHolidayName AS [Description]
		FROM [Profiler].[func_PublicHolidaysByYear](DATEPART(YEAR, @FromDate))
		WHERE PublicHolidayDate BETWEEN @FromDate
				AND @ToDate		
		UNION		
		SELECT 0 AS [HolidayId]
			,[PublicHolidayDate] AS [Date]
			,PublicHolidayName AS [Description]
		FROM [Profiler].[func_PublicHolidaysByYear](DATEPART(YEAR, @ToDate))
		WHERE PublicHolidayDate BETWEEN @FromDate
				AND @ToDate
	ELSE
		SELECT 0 AS [HolidayId]
			,[PublicHolidayDate] AS [Date]
			,PublicHolidayName AS [Description]
		FROM [Profiler].[func_PublicHolidaysByYear](DATEPART(YEAR, GETDATE()))
END
