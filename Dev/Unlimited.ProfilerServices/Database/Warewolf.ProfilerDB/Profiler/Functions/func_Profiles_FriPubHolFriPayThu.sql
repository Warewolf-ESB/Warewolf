CREATE FUNCTION [Profiler].[func_Profiles_FriPubHolFriPayThu] (
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
		,day(FridayDebitRunDate) AS DebitRunID
		,datename(weekday, FridayDebitRunDate) AS DebitRunIDDayName
		,FridayDebitRunDate AS DebitRunIDDate
		,datename(month, FridayDebitRunDate) AS DebitRunIDMonthName
		,'Fri-PubHolFri-PayThu' AS LUCCollectionProfile
	FROM (
		SELECT (
				SELECT [DateName]
				FROM @MasterDates
				WHERE DateName = DATEADD(DAY, 1, d.DateName)
					AND PubHolFlag = 1
				) FridayDebitRunDate
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
		WHERE [DayName] = 'Thursday'
			AND PubHolFlag = 0
		) dates
	WHERE FridayDebitRunDate IS NOT NULL
	
	UNION ALL
	
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
		,'37' DebitRunID --First Friday
		,datename(weekday, FridayDebitRunDate) AS DebitRunIDDayName
		,FridayDebitRunDate AS DebitRunIDDate
		,datename(month, FridayDebitRunDate) AS DebitRunIDMonthName
		,'Fri-PubHolFri-PayThu' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY Friday.MonthName
				,Friday.YearNo ORDER BY Friday.MonthName
					,Friday.YearNo
					,Friday.DayOfMonth
				) DayTypeID
			,Thursday.[DateName]
			,Thursday.[DayName]
			,Thursday.[MonthName]
			,Thursday.[DayOfWeek]
			,Thursday.[DayOfMonth]
			,Thursday.[MonthOfYear]
			,Thursday.[YearNo]
			,Thursday.[MonthStartDate]
			,Thursday.[MonthEndDate]
			,Thursday.[PubHolFlag]
			,Thursday.[WeekDayFlag]
			,Thursday.[WeekStartDate]
			,Thursday.[WeekEndDate]
			,Friday.DateName FridayDebitRunDate
		FROM @MasterDates Friday
		LEFT OUTER JOIN @MasterDates Thursday ON Friday.DateName = dateadd(day, 1, Thursday.DateName)
			AND Friday.PubHolFlag = 1
		WHERE Friday.DayName = 'Friday'
		) Dates
	WHERE DayTypeID = 1
		AND PubHolFlag = 0
	
	UNION ALL
	
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
		,'35' DebitRunID --Second Friday
		,datename(weekday, FridayDebitRunDate) AS DebitRunIDDayName
		,FridayDebitRunDate AS DebitRunIDDate
		,datename(month, FridayDebitRunDate) AS DebitRunIDMonthName
		,'Fri-PubHolFri-PayThu' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY Friday.MonthName
				,Friday.YearNo ORDER BY Friday.MonthName
					,Friday.YearNo
					,Friday.DayOfMonth
				) DayTypeID
			,Thursday.[DateName]
			,Thursday.[DayName]
			,Thursday.[MonthName]
			,Thursday.[DayOfWeek]
			,Thursday.[DayOfMonth]
			,Thursday.[MonthOfYear]
			,Thursday.[YearNo]
			,Thursday.[MonthStartDate]
			,Thursday.[MonthEndDate]
			,Thursday.[PubHolFlag]
			,Thursday.[WeekDayFlag]
			,Thursday.[WeekStartDate]
			,Thursday.[WeekEndDate]
			,Friday.DateName FridayDebitRunDate
		FROM @MasterDates Friday
		LEFT OUTER JOIN @MasterDates Thursday ON Friday.DateName = dateadd(day, 1, Thursday.DateName)
			AND Friday.PubHolFlag = 1
		WHERE Friday.DayName = 'Friday'
		) Dates
	WHERE DayTypeID = 2
		AND PubHolFlag = 0
	
	UNION ALL
	
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
		,'33' DebitRunID --THIRD FRIDAY
		,datename(weekday, FridayDebitRunDate) AS DebitRunIDDayName
		,FridayDebitRunDate AS DebitRunIDDate
		,datename(month, FridayDebitRunDate) AS DebitRunIDMonthName
		,'Fri-PubHolFri-PayThu' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY Friday.MonthName
				,Friday.YearNo ORDER BY Friday.MonthName
					,Friday.YearNo
					,Friday.DayOfMonth
				) DayTypeID
			,Thursday.[DateName]
			,Thursday.[DayName]
			,Thursday.[MonthName]
			,Thursday.[DayOfWeek]
			,Thursday.[DayOfMonth]
			,Thursday.[MonthOfYear]
			,Thursday.[YearNo]
			,Thursday.[MonthStartDate]
			,Thursday.[MonthEndDate]
			,Thursday.[PubHolFlag]
			,Thursday.[WeekDayFlag]
			,Thursday.[WeekStartDate]
			,Thursday.[WeekEndDate]
			,Friday.DateName FridayDebitRunDate
		FROM @MasterDates Friday
		LEFT OUTER JOIN @MasterDates Thursday ON Friday.DateName = dateadd(day, 1, Thursday.DateName)
			AND Friday.PubHolFlag = 1
		WHERE Friday.DayName = 'Friday'
		) Dates
	WHERE DayTypeID = 3
		AND PubHolFlag = 0
	
	UNION ALL
	
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
		,'34' AS DebitRunID --FOURTH FRIDAY
		,datename(weekday, FridayDebitRunDate) AS DebitRunIDDayName
		,FridayDebitRunDate AS DebitRunIDDate
		,datename(month, FridayDebitRunDate) AS DebitRunIDMonthName
		,'Fri-PubHolFri-PayThu' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY Friday.MonthName
				,Friday.YearNo ORDER BY Friday.MonthName
					,Friday.YearNo
					,Friday.DayOfMonth
				) DayTypeID
			,Thursday.[DateName]
			,Thursday.[DayName]
			,Thursday.[MonthName]
			,Thursday.[DayOfWeek]
			,Thursday.[DayOfMonth]
			,Thursday.[MonthOfYear]
			,Thursday.[YearNo]
			,Thursday.[MonthStartDate]
			,Thursday.[MonthEndDate]
			,Thursday.[PubHolFlag]
			,Thursday.[WeekDayFlag]
			,Thursday.[WeekStartDate]
			,Thursday.[WeekEndDate]
			,Friday.DateName FridayDebitRunDate
		FROM @MasterDates Friday
		LEFT OUTER JOIN @MasterDates Thursday ON Friday.DateName = dateadd(day, 1, Thursday.DateName)
			AND Friday.PubHolFlag = 1
		WHERE Friday.DayName = 'Friday'
		) Dates
	WHERE DayTypeID = 4
		AND PubHolFlag = 0
	
	UNION ALL
	
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
		,'32' DebitRunID --LAST FRIDAY
		,datename(weekday, FridayDebitRunDate) AS DebitRunIDDayName
		,FridayDebitRunDate AS DebitRunIDDate
		,datename(month, FridayDebitRunDate) AS DebitRunIDMonthName
		,'Fri-PubHolFri-PayThu' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY Friday.MonthName
				,Friday.YearNo ORDER BY Friday.MonthName
					,Friday.YearNo
					,Friday.DayOfMonth DESC
				) DayTypeID
			,Thursday.[DateName]
			,Thursday.[DayName]
			,Thursday.[MonthName]
			,Thursday.[DayOfWeek]
			,Thursday.[DayOfMonth]
			,Thursday.[MonthOfYear]
			,Thursday.[YearNo]
			,Thursday.[MonthStartDate]
			,Thursday.[MonthEndDate]
			,Thursday.[PubHolFlag]
			,Thursday.[WeekDayFlag]
			,Thursday.[WeekStartDate]
			,Thursday.[WeekEndDate]
			,Friday.DateName FridayDebitRunDate
		FROM @MasterDates Friday
		LEFT OUTER JOIN @MasterDates Thursday ON Friday.DateName = dateadd(day, 1, Thursday.DateName)
			AND Friday.PubHolFlag = 1
		WHERE Friday.DayName = 'Friday'
		) Dates
	WHERE DayTypeID = 1
		AND PubHolFlag = 0
	
	UNION ALL
	
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
		,'31' DebitRunID --LAST WORKING DAY	
		,datename(weekday, LastDayDebitRunDate) AS DebitRunIDDayName
		,LastDayDebitRunDate AS DebitRunIDDate
		,datename(month, LastDayDebitRunDate) AS DebitRunIDMonthName
		,'Fri-PubHolFri-PayThu' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY LastDay.MonthName
				,LastDay.YearNo ORDER BY LastDay.MonthName
					,LastDay.YearNo
					,LastDay.DayOfMonth DESC
				) DateFlag
			,Thursday.[DateName]
			,Thursday.[DayName]
			,Thursday.[MonthName]
			,Thursday.[DayOfWeek]
			,Thursday.[DayOfMonth]
			,Thursday.[MonthOfYear]
			,Thursday.[YearNo]
			,Thursday.[MonthStartDate]
			,Thursday.[MonthEndDate]
			,Thursday.[PubHolFlag]
			,Thursday.[WeekDayFlag]
			,Thursday.[WeekStartDate]
			,Thursday.[WeekEndDate]
			,LastDay.DateName LastDayDebitRunDate
		FROM @MasterDates LastDay
		LEFT OUTER JOIN @MasterDates Thursday ON LastDay.DateName = dateadd(day, 1, Thursday.DateName)
			AND LastDay.DayName = 'Thursday'
			AND LastDay.PubHolFlag = 1
		WHERE LastDay.WeekDayFlag = 1
		ORDER BY YearNo
			,MonthOfYear
			,DayOfMonth
		) dates
	WHERE DateFlag = 1
		AND PubHolFlag = 0

	RETURN
END
