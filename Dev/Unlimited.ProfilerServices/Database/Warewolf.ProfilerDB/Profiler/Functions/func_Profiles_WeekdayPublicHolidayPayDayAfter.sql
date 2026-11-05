CREATE FUNCTION [Profiler].[func_Profiles_WeekdayPublicHolidayPayDayAfter] (
	@FromDate DATE
	,@Todate DATE
	)
RETURNS @DATES TABLE (
	[DateName] DATETIME
	,[DayName] VARCHAR(50)
	,[MonthName] VARCHAR(50)
	,[DayOfWeek] SMALLINT
	,[DayOfMonth] SMALLINT
	,[MonthOfYear] SMALLINT
	,[YearNo] SMALLINT
	,[MonthStartDate] DATE
	,[MonthEndDate] DATE
	,[PubHolFlag] BIT
	,[WeekDayFlag] BIT
	,[WeekStartDate] DATE
	,[WeekEndDate] DATE
	,DebitRunID SMALLINT
	,DebitRunIDDayName VARCHAR(50)
	,DebitRunIDDate DATE
	,DebitRunIDMonthName VARCHAR(50)
	,LUCCollectionProfile VARCHAR(50)
	)
AS
BEGIN
	DECLARE @MasterDates TABLE (
		[DateName] DATE
		,[DayName] VARCHAR(50)
		,[MonthName] VARCHAR(50)
		,[DayOfWeek] SMALLINT
		,[DayOfMonth] SMALLINT
		,[MonthOfYear] SMALLINT
		,[YearNo] SMALLINT
		,[MonthStartDate] DATE
		,[MonthEndDate] DATE
		,[PubHolFlag] BIT
		,[WeekDayFlag] BIT
		,[WeekStartDate] DATE
		,[WeekEndDate] DATE
		)

	INSERT INTO @MasterDates
	SELECT [DateName]
		,[DayName]
		,[MonthName]
		,[DayOfWeek]
		,[DayOfMonth]
		,[MonthOfYear]
		,[YearNo]
		,[MonthStartDate]
		,[MonthEndDate]
		,[PubHolFlag]
		,[WeekDayFlag]
		,[WeekStartDate]
		,[WeekEndDate]
	FROM [Profiler].[func_MasterDates](@FromDate, @ToDate)


	INSERT INTO @DATES
	SELECT [DateName]
		,[DayName]
		,[MonthName]
		,[DayOfWeek]
		,[DayOfMonth]
		,[MonthOfYear]
		,[YearNo]
		,[MonthStartDate]
		,[MonthEndDate]
		,[PubHolFlag]
		,[WeekDayFlag]
		,[WeekStartDate]
		,[WeekEndDate]
		,day(PublichHolidayDRI) AS DebitRunID
		,datename(weekday, PublichHolidayDRI) AS DebitRunIDDayName
		,PublichHolidayDRI AS DebitRunIDDate
		,datename(month, PublichHolidayDRI) AS DebitRunIDMonthName
		,'Weekday-PublicHoliday-PayDayAfter' AS LUCCollectionProfile
	FROM (
		SELECT (
				SELECT [DateName]
				FROM @MasterDates
				WHERE DateName = DATEADD(DAY, - 1, d.DateName)
					AND PubHolFlag = 1
				) PublichHolidayDRI
			,[DateName]
			,[DayName]
			,[MonthName]
			,[DayOfWeek]
			,[DayOfMonth]
			,[MonthOfYear]
			,[YearNo]
			,[MonthStartDate]
			,[MonthEndDate]
			,[PubHolFlag]
			,[WeekDayFlag]
			,[WeekStartDate]
			,[WeekEndDate]
		FROM @MasterDates d
		WHERE d.[WeekDayFlag] = 1
			AND d.DayName IN (
				'Wednesday'
				,'Thursday'
				,'Friday'
				)
			AND PubHolFlag = 0
		) dates
	WHERE PublichHolidayDRI IS NOT NULL

	RETURN
END
