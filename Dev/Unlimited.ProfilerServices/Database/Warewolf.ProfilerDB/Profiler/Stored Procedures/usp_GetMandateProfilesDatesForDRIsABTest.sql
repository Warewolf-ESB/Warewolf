CREATE PROCEDURE [Profiler].[usp_GetMandateProfilesDatesForDRIsABTest] (
	@MandateId UNIQUEIDENTIFIER
	,@PaymentCycle INT
	,@FromDebitRunId INT
	,@ToDebitRunId INT
	,@Day VARCHAR(2)
	)
AS
SET NOCOUNT ON

BEGIN
	DECLARE @EndOfTracking TABLE (
		CollectionDate DATE
		,DebitRunId INT
		,PaymentCycle INT
		);

	INSERT INTO @EndOfTracking (
		CollectionDate
		,DebitRunId
		,PaymentCycle
		)
	SELECT TOP 1 pd.CollectionDate
		,pd.[DebitRunId]
		,pd.[PaymentCycle]
	FROM Profiler.ProfileDates pd WITH (NOLOCK)
	LEFT OUTER JOIN Profiler.MandateScores ms WITH (NOLOCK) ON ms.DebitRunId = pd.DebitRunId
		AND ms.ProfileId = pd.ProfileId
		AND ms.MandateId = @MandateId
	WHERE pd.DebitRunId IN (
			@FromDebitRunId
			,@ToDebitRunId
			)
		AND pd.PaymentCycle = @PaymentCycle
		AND datepart(month, pd.CollectionDate) > datepart(month, CONVERT(DATE, cast(pd.PaymentCycle AS VARCHAR(6)) + '01', 112))
		AND datepart(day, pd.CollectionDate) <= @Day
	ORDER BY pd.CollectionDate DESC

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
		AND pd.CollectionDate <= DATEADD(MONTH, 1, CONVERT(DATE, CAST(pd.PaymentCycle AS VARCHAR) + @Day, 102))
		AND pd.ProfileId NOT LIKE 'LastDay%'
	
	UNION ALL
	
	SELECT NULL AS [ProfileDateId]
		,CollectionDate
		,[DebitRunId]
		,[PaymentCycle]
		,'EndOfTracking' AS [ProfileId]
		,NULL AS [Score]
		,NULL AS [ScoreAdjustmentReason]
	FROM @EndOfTracking
END

RETURN 0
