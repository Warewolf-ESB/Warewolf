CREATE PROCEDURE [Profiler].[usp_ApplyRelatedMandatesScoring] @TargetMandateId UNIQUEIDENTIFIER
	,@RelatedMandates VARCHAR(MAX)
	,@PaymentCycle INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @SystemDate DATETIME = GETDATE();

	WITH [PostiveScoreCheck] ([MandateId])
	AS (
		SELECT DISTINCT [MandateFilter].[MandateId]
		FROM [Profiler].[MandateScores] AS [MandateFilter] WITH (NOLOCK)
		WHERE [MandateFilter].[MandateId] IN (
				SELECT value
				FROM STRING_SPLIT(@RelatedMandates, ',')
				)
			AND [MandateFilter].[Score] > 0
			AND CAST([MandateFilter].[PaymentCycle] AS INT) >= @PaymentCycle
			AND [MandateFilter].[ScoreAdjustmentReason] NOT LIKE '%ALGN%'
		)
		,[MandateScoresToCopy] (
		[TargetMandateId]
		,[MandateId]
		,[ProfileId]
		,[DebitRunId]
		,[PaymentCycle]
		,[ScoredOn]
		,[Score]
		,[ScoreAdjustedOn]
		,[ScoreAdjustmentReason]
		)
	AS (
		SELECT [TargetMandateId]
			,[MandateId]
			,[ProfileId]
			,[DebitRunId]
			,[PaymentCycle]
			,[ScoredOn]
			,[Score]
			,[ScoreAdjustedOn]
			,[ScoreAdjustmentReason]
		FROM (
			SELECT @TargetMandateId AS TargetMandateId
				,[ScoresToCopy].[MandateId]
				,[ScoresToCopy].[ProfileId]
				,[ScoresToCopy].[DebitRunId]
				,[ScoresToCopy].[PaymentCycle]
				,[ScoresToCopy].[ScoredOn]
				,[ScoresToCopy].[Score]
				,[ScoresToCopy].[ScoreAdjustedOn]
				,[ScoresToCopy].[ScoreAdjustmentReason]
				,ROW_NUMBER() OVER (
					PARTITION BY [ScoresToCopy].[ProfileId]
					,[ScoresToCopy].[DebitRunId] ORDER BY [ScoresToCopy].[Score] DESC
						,COALESCE([ScoresToCopy].[ScoreAdjustedOn], COALESCE([ScoresToCopy].[ScoredOn], 0)) DESC
					) AS ranking
				,COALESCE([ScoresToCopy].[ScoreAdjustedOn], [ScoresToCopy].[ScoredOn]) CopyScoredOn
				,COALESCE([TargetMandateScores].[ScoreAdjustedOn], COALESCE([TargetMandateScores].[ScoredOn], 0)) TargetScoredOn
			FROM [Profiler].[MandateScores] AS [ScoresToCopy] WITH (NOLOCK)
			INNER JOIN [PostiveScoreCheck] ON [PostiveScoreCheck].[MandateId] = [ScoresToCopy].[MandateId]
				AND [PostiveScoreCheck].[MandateId] <> @TargetMandateId
			LEFT OUTER JOIN [Profiler].[MandateScores] AS [TargetMandateScores] WITH (NOLOCK) ON [TargetMandateScores].[ProfileId] = [ScoresToCopy].[ProfileId]
				AND [TargetMandateScores].[DebitRunId] = [ScoresToCopy].[DebitRunId]
				AND [TargetMandateScores].[MandateId] = @TargetMandateId
			WHERE [ScoresToCopy].[ScoreAdjustmentReason] NOT LIKE '%' + CONVERT(NVARCHAR(36), @TargetMandateId) + '%'
			) AS RankedData
		WHERE ranking = 1
		)
	MERGE [Profiler].[MandateScores] WITH (
			ROWLOCK
			,UPDLOCK
			) AS [MandateScores]
	USING MandateScoresToCopy
		ON MandateScoresToCopy.[ProfileId] = [MandateScores].[ProfileId]
			AND MandateScoresToCopy.[DebitRunId] = [MandateScores].[DebitRunId]
			AND MandateScoresToCopy.TargetMandateId = [MandateScores].[MandateId]
	WHEN MATCHED
		THEN
			UPDATE
			SET [MandateScores].[Score] = MandateScoresToCopy.[Score]
				,[MandateScores].[PaymentCycle] = @PaymentCycle
				,[MandateScores].[ScoreAdjustedOn] = @SystemDate
				,[MandateScores].[ScoreAdjustmentReason] = 'ALGN Scoring Updated from ' + CAST(MandateScoresToCopy.[MandateId] AS VARCHAR(MAX)) + ' For ' + CAST(@PaymentCycle AS VARCHAR)
	WHEN NOT MATCHED BY TARGET
		THEN
			INSERT (
				[MandateId]
				,[ProfileId]
				,[DebitRunId]
				,[PaymentCycle]
				,[ScoredOn]
				,[Score]
				,[ScoreAdjustedOn]
				,[ScoreAdjustmentReason]
				)
			VALUES (
				@TargetMandateId
				,MandateScoresToCopy.[ProfileId]
				,MandateScoresToCopy.DebitRunId
				,@PaymentCycle
				,@SystemDate
				,MandateScoresToCopy.[Score]
				,@SystemDate
				,'ALGN Scoring Copied from ' + cast(MandateScoresToCopy.[MandateId] AS VARCHAR(MAX)) + ' For ' + CAST(@PaymentCycle AS VARCHAR)
				)
	OUTPUT $ACTION AS [Action]
		,[INSERTED].[MandateId]
		,[INSERTED].[ProfileId]
		,[INSERTED].[DebitRunId]
		,[INSERTED].[PaymentCycle]
		,[INSERTED].[ScoredOn]
		,[INSERTED].[Score]
		,[INSERTED].[ScoreAdjustedOn]
		,[INSERTED].[ScoreAdjustmentReason];
END

RETURN 0
