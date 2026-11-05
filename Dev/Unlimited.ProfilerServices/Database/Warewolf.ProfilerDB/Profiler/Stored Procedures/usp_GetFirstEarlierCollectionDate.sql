CREATE PROCEDURE Profiler.usp_GetFirstEarlierCollectionDate (
	@Date DATE
	,@ForPaymentCycle INT
	,@IsWeekDay BIT
	,@TrackingDays INT = 10
	)
AS
BEGIN
	SET NOCOUNT ON
 
	DECLARE @CollectionDate DATE,
	@DebitRunId INT,
	@GrantCollectionDate DATE,
	@TrackingEndDate DATE;

	DECLARE @NextPaymentCycle INT = CONVERT(INT, FORMAT(DATEADD(MONTH, 1, CONVERT(DATE, CAST(@ForPaymentCycle AS CHAR(6)) + '01', 102)), 'yyyyMM'));
	DECLARE @NextMaxGrantDate DATE

	SELECT TOP 1 @NextMaxGrantDate = [CollectionDate]
	FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
	INNER JOIN [Profiler].[GrantDates] gd WITH (NOLOCK) ON gd.[Date] = pd.[CollectionDate] and gd.PaymentCycle = pd.PaymentCycle
	WHERE pd.[PaymentCycle] = @NextPaymentCycle
	ORDER BY gd.[Date] DESC

	DECLARE @MaxTrackingDate DATE
	SELECT TOP 1 @MaxTrackingDate = ISNULL(@NextMaxGrantDate, pd.[CollectionDate])
	FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
	WHERE pd.[PaymentCycle] = @NextPaymentCycle
		AND pd.[CollectionDate] <= DATEADD(MONTH, 1, CONVERT(DATE, CAST(@ForPaymentCycle AS CHAR(6)) + '07', 102))
	ORDER BY pd.[CollectionDate] DESC
 
	SELECT TOP 1 @CollectionDate = CollectionDate,@DebitRunId = DebitRunId
	FROM [Profiler].[ProfileDates] WITH (NOLOCK)
	WHERE CollectionDate < @Date
		AND PaymentCycle <= @ForPaymentCycle
		AND (
        @IsWeekDay = 0
        OR ProfileId = 'Weekday'
        )
	ORDER BY CollectionDate DESC
 
	SET @TrackingEndDate = DATEADD(DAY, @TrackingDays - 1, @CollectionDate);

	SELECT TOP 1 @CollectionDate AS CollectionDate
		,pd.[ProfileDateId]
		,CASE WHEN pd.[CollectionDate] >= @MaxTrackingDate THEN @MaxTrackingDate ELSE pd.[CollectionDate] END AS EndofTrackingDate
		,DATEDIFF(DAY,@CollectionDate,CASE WHEN pd.[CollectionDate] >= @MaxTrackingDate THEN @MaxTrackingDate ELSE pd.[CollectionDate] END) + 1 AS TrackingDays
		,@DebitRunId as CollectionDateDRI
		,pd.[PaymentCycle]
		,pd.[ProfileId]
	FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
	INNER JOIN Profiler.Profiles p WITH (NOLOCK) on p.ProfileId = pd.ProfileId and p.IsDeleted = 0
	INNER JOIN Profiler.DebitRunIds d WITH (NOLOCK) on d.DebitRunId = pd.DebitRunId and d.IsDeleted = 0
	LEFT JOIN Profiler.GrantDates PG WITH (NOLOCK) on PG.PaymentCycle = pd.PaymentCycle
	WHERE CollectionDate <= @TrackingEndDate
	ORDER BY pd.CollectionDate DESC
END
