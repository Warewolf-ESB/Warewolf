CREATE FUNCTION [Profiler].[func_Profiles_SatPubHolFriPayMon] (
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
		,day(SaturdayDebitRunDate) AS DebitRunID
		,datename(weekday, SaturdayDebitRunDate) AS DebitRunIDDayName
		,SaturdayDebitRunDate AS DebitRunIDDate
		,datename(month, SaturdayDebitRunDate) AS DebitRunIDMonthName
		,'Sat-PubHolFri-PayMon' AS LUCCollectionProfile
	FROM (
		SELECT (
				SELECT [DateName]
				FROM @MasterDates
				WHERE DateName = DATEADD(DAY, - 2, d.DateName)
					AND PubHolFlag = 0
				) SaturdayDebitRunDate
			,(
				SELECT DayOfMonth
				FROM @MasterDates
				WHERE DateName = DATEADD(DAY, - 3, d.DateName)
					AND PubHolFlag = 1
				) FridayDebitRunID
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
		WHERE DayName = 'Monday'
			AND PubHolFlag = 0
		) dates
	WHERE FridayDebitRunID IS NOT NULL
		AND SaturdayDebitRunDate IS NOT NULL

	RETURN
END
