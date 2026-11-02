CREATE FUNCTION [Profiler].[func_Profiles_FriPubHolThuFriPayWed] (
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
		,'Fri-PubHolThuFri-PayWed' AS LUCCollectionProfile
	FROM (
		SELECT (SELECT [DateName] FROM @MasterDates Where DateName = DATEADD(DAY,1,d.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
			,(SELECT [DateName] FROM @MasterDates WHERE DateName = DATEADD(DAY,2,d.DateName) AND PubHolFlag = 1) FridayDebitRunDate
			,(SELECT [DateName] FROM @MasterDates WHERE DateName = DATEADD(DAY,3,d.DateName) AND PubHolFlag = 0 ) SaturdayDebitRunDate
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
		WHERE DayName = 'Wednesday'
			AND PubHolFlag = 0
		) dates
	WHERE SaturdayDebitRunDate IS NOT NULL
		AND FridayDebitRunDate IS NOT NULL
		AND ThursDayDebitRunDate IS NOT NULL


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
		,'Fri-PubHolThuFri-PayWed' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY Friday.MonthName
				,Friday.YearNo ORDER BY Friday.MonthName
					,Friday.YearNo
					,Friday.DayOfMonth
				) DayTypeID
			,(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,- 1,Friday.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
			,WednesDay.[DateName]
			,WednesDay.[DayName]
			,WednesDay.[MonthName]
			,WednesDay.[DayOfWeek]
			,WednesDay.[DayOfMonth]
			,WednesDay.[MonthOfYear]
			,WednesDay.[YearNo]
			,WednesDay.[MonthStartDate]
			,WednesDay.[MonthEndDate]
			,WednesDay.[PubHolFlag]
			,WednesDay.[WeekDayFlag]
			,WednesDay.[WeekStartDate]
			,WednesDay.[WeekEndDate]
			,Friday.DateName FridayDebitRunDate
		FROM @MasterDates Friday
		LEFT OUTER JOIN @MasterDates WednesDay ON Friday.DateName = dateadd(day, 2, WednesDay.DateName)
			AND Friday.PubHolFlag = 1
		WHERE Friday.DayName = 'Friday'
		) Dates
	WHERE DayTypeID = 1
		AND PubHolFlag = 0
		AND ThursDayDebitRunDate IS NOT NULL
	
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
		,'Fri-PubHolThuFri-PayWed' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY Friday.MonthName
				,Friday.YearNo ORDER BY Friday.MonthName
					,Friday.YearNo
					,Friday.DayOfMonth
				) DayTypeID
			,(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,- 1,Friday.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
			,WednesDay.[DateName]
			,WednesDay.[DayName]
			,WednesDay.[MonthName]
			,WednesDay.[DayOfWeek]
			,WednesDay.[DayOfMonth]
			,WednesDay.[MonthOfYear]
			,WednesDay.[YearNo]
			,WednesDay.[MonthStartDate]
			,WednesDay.[MonthEndDate]
			,WednesDay.[PubHolFlag]
			,WednesDay.[WeekDayFlag]
			,WednesDay.[WeekStartDate]
			,WednesDay.[WeekEndDate]
			,Friday.DateName FridayDebitRunDate
		FROM @MasterDates Friday
		LEFT OUTER JOIN @MasterDates WednesDay ON Friday.DateName = dateadd(day, 2, WednesDay.DateName)
			AND Friday.PubHolFlag = 1
		WHERE Friday.DayName = 'Friday'
		) Dates
	WHERE DayTypeID = 2
		AND PubHolFlag = 0
		AND ThursDayDebitRunDate IS NOT NULL
	
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
		,'Fri-PubHolThuFri-PayWed' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY Friday.MonthName
				,Friday.YearNo ORDER BY Friday.MonthName
					,Friday.YearNo
					,Friday.DayOfMonth
				) DayTypeID
			,(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,- 1,Friday.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
			,WednesDay.[DateName]
			,WednesDay.[DayName]
			,WednesDay.[MonthName]
			,WednesDay.[DayOfWeek]
			,WednesDay.[DayOfMonth]
			,WednesDay.[MonthOfYear]
			,WednesDay.[YearNo]
			,WednesDay.[MonthStartDate]
			,WednesDay.[MonthEndDate]
			,WednesDay.[PubHolFlag]
			,WednesDay.[WeekDayFlag]
			,WednesDay.[WeekStartDate]
			,WednesDay.[WeekEndDate]
			,Friday.DateName FridayDebitRunDate
		FROM @MasterDates Friday
		LEFT OUTER JOIN @MasterDates WednesDay ON Friday.DateName = dateadd(day, 2, WednesDay.DateName)
			AND Friday.PubHolFlag = 1
		WHERE Friday.DayName = 'Friday'
		) Dates
	WHERE DayTypeID = 3
		AND PubHolFlag = 0
		AND ThursDayDebitRunDate IS NOT NULL
	
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
		,'Fri-PubHolThuFri-PayWed' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY Friday.MonthName
				,Friday.YearNo ORDER BY Friday.MonthName
					,Friday.YearNo
					,Friday.DayOfMonth
				) DayTypeID
			,(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,- 1,Friday.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
			,WednesDay.[DateName]
			,WednesDay.[DayName]
			,WednesDay.[MonthName]
			,WednesDay.[DayOfWeek]
			,WednesDay.[DayOfMonth]
			,WednesDay.[MonthOfYear]
			,WednesDay.[YearNo]
			,WednesDay.[MonthStartDate]
			,WednesDay.[MonthEndDate]
			,WednesDay.[PubHolFlag]
			,WednesDay.[WeekDayFlag]
			,WednesDay.[WeekStartDate]
			,WednesDay.[WeekEndDate]
			,Friday.DateName FridayDebitRunDate
		FROM @MasterDates Friday
		LEFT OUTER JOIN @MasterDates WednesDay ON Friday.DateName = dateadd(day, 2, WednesDay.DateName)
			AND Friday.PubHolFlag = 1
		WHERE Friday.DayName = 'Friday'
		) Dates
	WHERE DayTypeID = 4
		AND PubHolFlag = 0
		AND ThursDayDebitRunDate IS NOT NULL
	
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
		,'Fri-PubHolThuFri-PayWed' LUCCollectionProfile
	FROM (
		SELECT TOP 100 PERCENT ROW_NUMBER() OVER (
				PARTITION BY Friday.MonthName
				,Friday.YearNo ORDER BY Friday.MonthName
					,Friday.YearNo
					,Friday.DayOfMonth DESC
				) DayTypeID
			,(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,- 1,Friday.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
			,WednesDay.[DateName]
			,WednesDay.[DayName]
			,WednesDay.[MonthName]
			,WednesDay.[DayOfWeek]
			,WednesDay.[DayOfMonth]
			,WednesDay.[MonthOfYear]
			,WednesDay.[YearNo]
			,WednesDay.[MonthStartDate]
			,WednesDay.[MonthEndDate]
			,WednesDay.[PubHolFlag]
			,WednesDay.[WeekDayFlag]
			,WednesDay.[WeekStartDate]
			,WednesDay.[WeekEndDate]
			,Friday.DateName FridayDebitRunDate
		FROM @MasterDates Friday
		LEFT OUTER JOIN @MasterDates WednesDay ON Friday.DateName = dateadd(day, 2, WednesDay.DateName)
			AND Friday.PubHolFlag = 1
		WHERE Friday.DayName = 'Friday'
		) Dates
	WHERE DayTypeID = 1
		AND PubHolFlag = 0
		AND ThursDayDebitRunDate IS NOT NULL

	RETURN
END
