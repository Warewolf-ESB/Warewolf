CREATE FUNCTION [Profiler].[func_Profiles_CrossApplyLastWorkingDayProfiles] (
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
		,LastWorkingDayDRIs.DebitRunId AS [DebitRunID]
		,[DebitRunIDDayName]
		,[DebitRunIDDate]
		,[DebitRunIDMonthName]
		,[LUCCollectionProfile]
	FROM (
		SELECT *
			,ROW_NUMBER() OVER (
				PARTITION BY [MonthOfYear]
				,[YearNo] ORDER BY [DateName] DESC
				) Ranking
		FROM [Profiler].func_Profiles_WeekDay(@FromDate, @ToDate)
		) AS ProfileDates
	CROSS JOIN (
		VALUES (42)
			,(43)
			,(44)
			,(45)
		) AS LastWorkingDayDRIs(DebitRunId)
	WHERE (
			LastWorkingDayDRIs.DebitRunId = 42
			AND Ranking = 2
			)
		OR (
			LastWorkingDayDRIs.DebitRunId = 43
			AND Ranking = 3
			)
		OR (
			LastWorkingDayDRIs.DebitRunId = 44
			AND Ranking = 4
			)
		OR (
			LastWorkingDayDRIs.DebitRunId = 45
			AND Ranking = 5
			)

	RETURN
END
