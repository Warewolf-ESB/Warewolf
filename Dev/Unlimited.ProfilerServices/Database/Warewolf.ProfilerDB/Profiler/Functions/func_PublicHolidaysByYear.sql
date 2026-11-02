CREATE FUNCTION [Profiler].[func_PublicHolidaysByYear] (@Year INT)
RETURNS @PublicHoliday TABLE (
	PublicHolidayDate DATE
	,PublicHolidayName VARCHAR(50)
	)
AS
/*
Created by:	Ian Marran
Date:		15 Aug 2013
Detail:		This function dynamically returns a table of South African public holidays for the given year. 
			I have commented the code below to explain the dates based on non fixed Easter Sunday.
Example:	SELECT * FROM General.func_PublicHolidaysByYear(2013)
Version History
Name			Log		Date			Detail
Ian Marran		52741	15 Aug 2013		Initial function creation
*/
BEGIN
	DECLARE @GoldenNumber INT
		,@Epact INT
		,@DaysTillPaschalFullMoon INT
		,@PaschalFullMoonDayOfWeek INT
		,@Centuary INT
		,@NumOfDaysToSunday INT
		,@EasterMonth INT
		,@EasterDay INT
		,@EasterSunday DATE

	-- Complex set of rules to calculate Easter Sunday... Who would have thought we would ever bring moon phases into SQL procs? :)
	-- Generally speaking, Easter falls on the first Sunday after the first Paschal Full Moon after the 21st of March
	SET @GoldenNumber = (@Year % 19) + 1 -- How far in the cycle is the moon/year relationship (relationship between the moon's phases and the days of the year repeats itself every 19 years)
	SET @Centuary = @Year / 100
	SET @Epact = (@Centuary - (@Centuary / 4) - (((8 * @Centuary) + 13) / 25) + (19 * (@GoldenNumber - 1)) + 15) % 30 -- Measure of the age of the moon (number of days that have passed since an "official" new moon)
	SET @DaysTillPaschalFullMoon = @Epact - (@Epact / 28) * (1 - (29 / (@Epact + 1)) * ((21 - (@GoldenNumber - 1)) / 11)) -- Now that we know Epact, we can work out the days till Paschal Full Moon
	SET @PaschalFullMoonDayOfWeek = (@Year + (@Year / 4) + @DaysTillPaschalFullMoon + 2 - @Centuary + (@Centuary / 4)) % 7 -- weekday for the Paschal full moon (0=Sunday, 1=Monday, etc.)
	SET @NumOfDaysToSunday = @DaysTillPaschalFullMoon - @PaschalFullMoonDayOfWeek -- number of days from 21 March to the Sunday on or before the Paschal full moon (between -6 and 28)
	SET @EasterMonth = 3 + (@NumOfDaysToSunday + 40) / 44 -- The month easter falls in (march or april)
	SET @EasterDay = @NumOfDaysToSunday + 28 - (31 * (@EasterMonth / 4)) -- the day in that month it falls on
	SET @EasterSunday = CAST(LEFT(@Year, 4) + '-' + RIGHT('0' + CAST(@EasterMonth AS VARCHAR(2)), 2) + '-' + RIGHT('0' + CAST(@EasterDay AS VARCHAR(2)), 2) AS DATE) -- Easter Sunday represented as a date
		;

	WITH ctePublicHolidays
	AS (
		SELECT CAST(LEFT(@Year, 4) + '-01-01' AS DATE) AS PublicHoliday
			,'New Years Day' AS PublicHolidayName
		
		UNION
		
		SELECT CAST(LEFT(@Year, 4) + '-03-21' AS DATE)
			,'Human Rights Day'
		
		UNION
		
		SELECT DATEADD(DAY, - 2, @EasterSunday)
			,'Good Friday' -- Friday before Easter
		
		UNION
		
		SELECT DATEADD(DAY, 1, @EasterSunday)
			,'Family Day' -- Monday after Easter
		
		UNION
		
		SELECT CAST(LEFT(@Year, 4) + '-04-27' AS DATE)
			,'Freedom Day'
		
		UNION
		
		SELECT CAST(LEFT(@Year, 4) + '-05-01' AS DATE)
			,'Workers Day'
		
		UNION
		
		SELECT CAST(LEFT(@Year, 4) + '-06-16' AS DATE)
			,'Youth Day'
		
		UNION
		
		SELECT CAST(LEFT(@Year, 4) + '-08-09' AS DATE)
			,'National Womans Day'
		
		UNION
		
		SELECT CAST(LEFT(@Year, 4) + '-09-24' AS DATE)
			,'Heritage Day'
		
		UNION
		
		SELECT CAST(LEFT(@Year, 4) + '-12-16' AS DATE)
			,'Day Of Reconciliation'
		
		UNION
		
		SELECT CAST(LEFT(@Year, 4) + '-12-25' AS DATE)
			,'Christmas Day'
		
		UNION
		
		SELECT CAST(LEFT(@Year, 4) + '-12-26' AS DATE)
			,'Day Of Goodwill'
		)
	INSERT INTO @PublicHoliday (
		PublicHolidayDate
		,PublicHolidayName
		)
	-- The Public Holidays Act (Act No 36 of 1994) determines that whenever any public holiday falls on a Sunday, the Monday following it will be a public holiday
	SELECT DATEADD(DAY, 1, PublicHoliday) AS PublicHoliday
		,'Public Holiday' AS PublicHolidayName
	FROM ctePublicHolidays
	WHERE DATENAME(WEEKDAY, PublicHoliday) = 'Sunday' -- does the day fall on a sunday
		AND PublicHolidayName <> 'Christmas Day' -- I know 26th is a holiday anyway so dont add a monday holiday ever (this happened in 2011 and so 27th was declared a holiday but will only be decided by government at the time it happens)
	
	UNION
	
	-- All the standard holidays
	SELECT PublicHoliday
		,PublicHolidayName
	FROM ctePublicHolidays
	
	UNION
	
	SELECT [Date]
		,[HolidayName]
	FROM Profiler.SpecialHolidays with (nolock) --Static list due to the nature of Ad-Hoc public holidays
	WHERE DATEPART(YEAR,[Date]) = @Year --Ad-Hoc public holidays only ever occur once in a specific year

	RETURN
END
