CREATE PROCEDURE [Profiler].[usp_CompareNewProfilesDatesToExistingDates]
(
	@FromDate DATE 
	,@Todate DATE
	)
	AS
	SET  NOCOUNT ON
SELECT 
'New Profile Dates' as [Status],
d.DateName
,d.DebitRunID
,cast(format(d.DebitRunIDDate,'yyyyMM') as int) PaymentCycle
,d.LUCCollectionProfile 
FROM [Profiler].[func_GetProfileDatesFromFunctions] (@FromDate,@ToDate) d
EXCEPT
Select 
'New Profile Dates' as [Status],
[CollectionDate]
      ,[DebitRunId]
      ,[PaymentCycle]
      ,[ProfileId]
From Profiler.ProfileDates pd with (nolock)
where [CollectionDate] between @FromDate and @ToDate


 UNION ALL
 

Select 
'Removed by New Profile Dates'  as [Status],
[CollectionDate]
      ,[DebitRunId]
      ,[PaymentCycle]
      ,[ProfileId]
From Profiler.ProfileDates pd with (nolock)
where [CollectionDate] between @FromDate and @ToDate
EXCEPT
SELECT 
'Removed by New Profile Dates'  as [Status],
d.DateName
,d.DebitRunID
,cast(format(d.DebitRunIDDate,'yyyyMM') as int)
,d.LUCCollectionProfile 
FROM [Profiler].[func_GetProfileDatesFromFunctions] (@FromDate,@ToDate) d
