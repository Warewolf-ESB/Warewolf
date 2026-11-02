CREATE PROCEDURE [Profiler].[usp_GetMandateScoredResults] @MandateId UNIQUEIDENTIFIER
	,@ForPaymentCycle INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @FilterScoreDatesTable TABLE (DebitRunId INT);

	INSERT INTO @FilterScoreDatesTable
	SELECT DISTINCT DebitRunId
	FROM Profiler.MandateScores WITH (NOLOCK)
	WHERE MandateId = @MandateId
		AND Score > 0
		AND ProfileId IN (
			'Weekday'
			,'Sat-PaySat'
			);

	WITH PositiveDRIScores (
		[DebitRunId]
		,[TotalScore]
		,[TotalScoreRanking]
		)
	AS (
		SELECT ms.[DebitRunId]
			,SUM([Score]) AS TotalScore
			,RANK() OVER (
				ORDER BY SUM([Score]) DESC
				) AS TotalScoreRanking
		FROM [Profiler].[MandateScores] ms WITH (NOLOCK)
		INNER JOIN [Profiler].[DebitRunIds] dri WITH (NOLOCK) ON dri.DebitRunId = ms.DebitRunId
		INNER JOIN [Profiler].[Profiles] profiles WITH (NOLOCK) ON profiles.ProfileId = ms.ProfileId
		INNER JOIN @FilterScoreDatesTable FT ON FT.DebitRunId = ms.DebitRunId
		WHERE [ms].[MandateId] = @MandateId
			AND [ms].[Score] > 0
			AND dri.IsDeleted = 0
			AND profiles.IsDeleted = 0
		GROUP BY ms.[DebitRunId]
		)
	SELECT [PositiveDRIScores].[DebitRunId]
		,[TotalScore]
		,(
			SELECT min([pd].[CollectionDate])
			FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
			WHERE [pd].[DebitRunId] = [PositiveDRIScores].[DebitRunId]
				AND [pd].[PaymentCycle] = @ForPaymentCycle
			) AS MinCollectionDate
		,(
			SELECT max([pd].[CollectionDate])
			FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
			WHERE [pd].[DebitRunId] = [PositiveDRIScores].[DebitRunId]
				AND [pd].[PaymentCycle] = @ForPaymentCycle
			) AS MaxCollectionDate
	FROM PositiveDRIScores
	WHERE [TotalScoreRanking] = 1
	
	UNION ALL
	
	SELECT DISTINCT ms.[DebitRunId]
		,[Score] AS TotalScore
		,(
			SELECT min([pd].[CollectionDate])
			FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
			WHERE [pd].[DebitRunId] = [ms].[DebitRunId]
				AND [pd].[PaymentCycle] = @ForPaymentCycle
			) AS MinCollectionDate
		,(
			SELECT max([pd].[CollectionDate])
			FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
			WHERE [pd].[DebitRunId] = [ms].[DebitRunId]
				AND [pd].[PaymentCycle] = @ForPaymentCycle
			) AS MaxCollectionDate
	FROM Profiler.MandateScores ms WITH (NOLOCK)
	INNER JOIN [Profiler].[DebitRunIds] dri WITH (NOLOCK) ON dri.DebitRunId = ms.DebitRunId
	INNER JOIN [Profiler].[Profiles] profiles WITH (NOLOCK) ON profiles.ProfileId = ms.ProfileId
	WHERE [ms].[MandateId] = @MandateId
		AND [ms].[Score] < 0
		AND dri.IsDeleted = 0
		AND profiles.IsDeleted = 0
		AND [ms].[ProfileId] = 'FullyCovered'
END
