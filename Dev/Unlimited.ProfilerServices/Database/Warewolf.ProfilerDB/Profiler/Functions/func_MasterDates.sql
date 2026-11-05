CREATE FUNCTION Profiler.func_MasterDates
(@FromDate DATE
,@ToDate DATE
)
RETURNS @DATES TABLE
(
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
AS

BEGIN
WITH    ListDates
          AS ( SELECT   @FromDate AS [DateName], 1 AS [level]
               UNION ALL
               SELECT   DATEADD(DAY, 1, [DateName]), [level] + 1
               FROM     ListDates
               WHERE    [DateName] < @ToDate )

INSERT INTO @DATES
		SELECT 			
		[DateName]
		,DATENAME(WEEKDAY,[DateName]) as [DayName]
		,DATENAME(MONTH,[DateName]) as [MonthName]
		,(DATEDIFF(dd,'17530107',[DateName])%7)+1  as [DayOfWeek]
		,DATEPART(DAY,[DateName]) as [DayOfMonth]
		,DATEPART(MONTH,[DateName]) as [MonthOfYear]
		,DATEPART(YEAR,[DateName]) as [YearNo]
		,dateadd(month,datediff(month,0,[DateName]),0) as [MonthStartDate]
		,dateadd(day,-1,dateadd(month,datediff(month,0,[DateName])+1,0)) as [MonthEndDate]
		,case when (select top 1 PublicHolidayDate from [Profiler].[func_PublicHolidaysByYear](DATEPART(YEAR,[DateName]))  where PublicHolidayDate = [DateName] order by PublicHolidayDate desc) is not null then 1 else 0 end as [PubHolFlag]
		,case when ((datediff(dd,'17530107',[DateName])%7)+1) between 2 and 6 then 1 else 0 end as [WeekDayFlag]
		,dateadd(dd,(datediff(dd,'17530101',[DateName])/7)*7,'17530101') as [WeekStartDate]
		,dateadd(dd,((datediff(dd,'17530101',[DateName])/7)*7)+6,'17530101') as [WeekEndDate]
	FROM	ListDates DateMaster OPTION (MAXRECURSION 0)
	RETURN
	END