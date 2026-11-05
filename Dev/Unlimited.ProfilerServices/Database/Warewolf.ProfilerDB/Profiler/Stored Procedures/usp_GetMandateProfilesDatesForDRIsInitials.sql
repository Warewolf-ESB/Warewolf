CREATE PROCEDURE [Profiler].[usp_GetMandateProfilesDatesForDRIsInitials] (
	@MandateId UNIQUEIDENTIFIER
	,@PaymentCycle INT
	,@FromDebitRunId INT
	,@ToDebitRunId INT
	,@StandardTracking AS INT = 10
	)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @EarliestDate AS DATE;

	SELECT @EarliestDate = MIN(pt.CollectionDate)
	FROM Profiler.ProfileDates pt
	WHERE pt.DebitRunId IN (
			@FromDebitRunId
			,@ToDebitRunId
			)
		AND pt.PaymentCycle = @PaymentCycle;
	
	DECLARE @NextPaymentCycle INT = CONVERT(INT, FORMAT(DATEADD(MONTH, 1, CONVERT(DATE, CAST(@PaymentCycle AS CHAR(6)) + '01', 102)), 'yyyyMM'));
	DECLARE @NextMaxGrantDate DATE

	SELECT @NextMaxGrantDate = MAX([CollectionDate])
	FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
	INNER JOIN [Profiler].[GrantDates] gd WITH (NOLOCK) ON gd.[Date] = pd.[CollectionDate] and gd.PaymentCycle = pd.PaymentCycle
	WHERE pd.[PaymentCycle] = @NextPaymentCycle;
	



	WITH TrackingDates (
		CollectionDate
		,TrackingDays
		,PaymentCycle
		)
	AS (
		SELECT DISTINCT TOP (1) pd.CollectionDate
			,DATEDIFF(DAY, @EarliestDate, pd.CollectionDate) + 1 AS TrackingDays
			,pd.PaymentCycle
		FROM Profiler.ProfileDates pd WITH (NOLOCK)
		LEFT OUTER JOIN Profiler.MandateScores ms WITH (NOLOCK) ON ms.DebitRunId = pd.DebitRunId
			AND ms.ProfileId = pd.ProfileId
			AND ms.MandateId = @MandateId
		WHERE pd.CollectionDate >= @EarliestDate
			AND pd.CollectionDate <= ISNULL(@NextMaxGrantDate,DATEADD(MONTH, 1, CONVERT(DATE, CAST(@PaymentCycle AS VARCHAR) + '07', 102)))
			AND DATEDIFF(DAY, @EarliestDate, pd.CollectionDate) + 1 <= @StandardTracking
			AND pd.PaymentCycle IN (@PaymentCycle, @NextPaymentCycle)
		ORDER BY pd.CollectionDate DESC
		)
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
	INNER JOIN Profiler.Profiles p WITH (NOLOCK) ON p.ProfileId = pd.ProfileId AND p.IsDeleted = 0
	LEFT OUTER JOIN Profiler.MandateScores ms WITH (NOLOCK) ON ms.DebitRunId = pd.DebitRunId
		AND ms.ProfileId = pd.ProfileId
		AND ms.MandateId = @MandateId
	WHERE pd.DebitRunId IN (
			@FromDebitRunId
			,@ToDebitRunId
			)
		AND pd.PaymentCycle = @PaymentCycle
		AND pd.CollectionDate <= ISNULL(@NextMaxGrantDate,DATEADD(MONTH, 1, CONVERT(DATE, CAST(pd.PaymentCycle AS VARCHAR) + '07', 102)))
	
	UNION ALL
	
	SELECT NULL AS [ProfileDateId]
		,CollectionDate
		,NULL AS [DebitRunId]
		,[PaymentCycle]
		,'EndOfTracking' AS [ProfileId]
		,NULL AS [Score]
		,NULL AS [ScoreAdjustmentReason]
	FROM TrackingDates
END
