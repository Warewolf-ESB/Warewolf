CREATE PROCEDURE [Profiler].[usp_GetMandateProfilesDatesForDRIs] (
	@MandateId UNIQUEIDENTIFIER
	,@PaymentCycle INT
	,@FromDebitRunId INT
	,@ToDebitRunId INT
	,@Day VARCHAR(2)
	)
AS
SET NOCOUNT ON

BEGIN
	DECLARE @NextPaymentCycle INT = CONVERT(INT, FORMAT(DATEADD(MONTH, 1, CONVERT(DATE, CAST(@PaymentCycle AS CHAR(6)) + '01', 102)), 'yyyyMM'));
	DECLARE @NextMaxGrantDate DATE

	SELECT TOP 1 @NextMaxGrantDate = [CollectionDate]
	FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
	INNER JOIN [Profiler].[GrantDates] gd WITH (NOLOCK) ON gd.[Date] = pd.[CollectionDate] and gd.PaymentCycle = pd.PaymentCycle
	WHERE pd.[PaymentCycle] = @NextPaymentCycle
	ORDER BY gd.[Date] DESC

	DECLARE @MaxTrackingDate DATE
	--Fail safe if no grant days, then use @DAY if @NextMaxGrantDate is null
	SELECT TOP 1 @MaxTrackingDate = ISNULL(@NextMaxGrantDate, pd.[CollectionDate])
	FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
	WHERE pd.[PaymentCycle] = @NextPaymentCycle
		AND pd.[CollectionDate] <= DATEADD(MONTH, 1, CONVERT(DATE, CAST(@PaymentCycle AS CHAR(6)) + @Day, 102))
	ORDER BY pd.[CollectionDate] DESC

	SELECT pd.[ProfileDateId]
		,pd.[CollectionDate]
		,pd.[DebitRunId]
		,pd.[PaymentCycle]
		,pd.[ProfileId]
		,CASE 
			WHEN ms.[Score] - (
					SELECT MAX(ts.[Score])
					FROM Profiler.MandateScores ts WITH (NOLOCK)
					WHERE ts.MandateId = ms.MandateId
						AND (
							ts.ProfileId <> ms.ProfileId
							OR ts.DebitRunId <> ms.DebitRunId
							)
					) >= 2
				THEN ms.[Score]
			ELSE NULL
			END AS [Score]
		,ms.[ScoreAdjustmentReason]
	FROM Profiler.ProfileDates pd WITH (NOLOCK)
	LEFT OUTER JOIN Profiler.MandateScores ms WITH (NOLOCK) ON ms.DebitRunId = pd.DebitRunId
		AND ms.ProfileId = pd.ProfileId
		AND ms.MandateId = @MandateId
	WHERE pd.DebitRunId IN (
			@FromDebitRunId
			,@ToDebitRunId
			)
		AND pd.PaymentCycle = @PaymentCycle
		AND pd.CollectionDate <= @MaxTrackingDate
		AND pd.ProfileId NOT LIKE 'LastDay%'
	
	UNION ALL
	--Create End-Of-Tracking date for DRI selection
	SELECT DISTINCT TOP 1 NULL AS [ProfileDateId]
		,CASE WHEN DATEADD(DAY, CASE WHEN pd.DebitRunId NOT BETWEEN 25 AND 31
							  THEN DATEDIFF(DAY,pd.CollectionDate,calcDate.MaxCollectionDate)
								WHEN pd.DebitRunId BETWEEN 25 AND 31
							  THEN DATEDIFF(DAY, pd.[CollectionDate], @MaxTrackingDate) END, pd.[CollectionDate]) > @MaxTrackingDate
				THEN @MaxTrackingDate
			ELSE DATEADD(DAY, CASE WHEN pd.DebitRunId NOT BETWEEN 25 AND 31
						THEN DATEDIFF(DAY,pd.CollectionDate,calcDate.MaxCollectionDate)
						WHEN pd.DebitRunId BETWEEN 25 AND 31 AND DATEDIFF(DAY, pd.[CollectionDate], @MaxTrackingDate) > 10
						THEN DATEDIFF(DAY, pd.[CollectionDate], calcDate.MaxCollectionDate)
						ELSE DATEDIFF(DAY, pd.[CollectionDate], @MaxTrackingDate)
						END, pd.[CollectionDate])
			END
		,pd.[DebitRunId]
		,pd.[PaymentCycle]
		,'EndOfTracking' AS [ProfileId]
		,NULL AS [Score]
		,NULL AS [ScoreAdjustmentReason]
	FROM Profiler.ProfileDates pd WITH (NOLOCK)
	OUTER APPLY (
    SELECT MAX(p2.CollectionDate) AS MaxCollectionDate -- return correct date check public holidays and sunday
    FROM Profiler.ProfileDates p2 WITH (NOLOCK)
    WHERE p2.PaymentCycle = pd.PaymentCycle
      AND p2.CollectionDate < DATEADD(DAY, 10, pd.CollectionDate)
	) calcDate
	LEFT OUTER JOIN Profiler.MandateScores ms WITH (NOLOCK) ON ms.DebitRunId = pd.DebitRunId
		AND ms.ProfileId = pd.ProfileId
		AND ms.MandateId = @MandateId
	WHERE pd.DebitRunId IN (
			@FromDebitRunId
			,@ToDebitRunId
			)
		AND pd.PaymentCycle = @PaymentCycle
	ORDER BY pd.CollectionDate ASC
END

RETURN 0
