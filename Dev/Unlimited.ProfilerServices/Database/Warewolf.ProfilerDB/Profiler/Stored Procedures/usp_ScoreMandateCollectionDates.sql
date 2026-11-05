CREATE PROCEDURE [Profiler].[usp_ScoreMandateCollectionDates] @MandateId UNIQUEIDENTIFIER
	,@ResultPaymentCycle INT
	,@ResultPresentedOnDate DATE
	,@ResultDate DATE
	,@ResultType VARCHAR(20)
	,@ResultMethod VARCHAR(50)
	,@MandateCanTrack BIT
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @NextPaymentCycle INT = CONVERT(INT, FORMAT(DATEADD(MONTH, 1, CONVERT(DATE, CAST(@ResultPaymentCycle AS CHAR(6)) + '01', 102)), 'yyyyMM'));
	DECLARE @NextPaymentDate DATE

	SET @NextPaymentDate = (
			SELECT ISNULL(MAX([Date]), CONCAT (
						LEFT(@NextPaymentCycle, 4)
						,'-'
						,RIGHT(@NextPaymentCycle, 2)
						,'-07'
						))
			FROM Profiler.GrantDates WITH (NOLOCK)
			WHERE PaymentCycle = @NextPaymentCycle
			)

	-- Score Collection Results
	MERGE [Profiler].[MandateScores] WITH (
			ROWLOCK
			,UPDLOCK
			) AS MandateScores
	USING (
		SELECT ProfileDateId
			,CollectionDate
			,pd.DebitRunId
			,pd.ProfileId
		FROM [Profiler].[ProfileDates] pd WITH (NOLOCK)
		LEFT OUTER JOIN [Profiler].[MandateScores] ms WITH (NOLOCK) ON ms.MandateId = @MandateId
			AND ms.PaymentCycle >= @ResultPaymentCycle
			AND pd.DebitRunId = ms.DebitRunId
			AND pd.ProfileId = ms.ProfileId
		WHERE pd.CollectionDate BETWEEN cast(@ResultPresentedOnDate AS DATE)
				AND cast(@ResultDate AS DATE)
			AND ms.MandateId IS NULL
		) AS [ProfileDates]
		ON [MandateScores].ProfileId = [ProfileDates].ProfileId
			AND [MandateScores].DebitRunId = [ProfileDates].DebitRunId
			AND [MandateScores].MandateId = @MandateId
	WHEN MATCHED
		THEN
			UPDATE
			SET [MandateScores].[Score] = CASE 
					WHEN @ResultType = 'Success'
						AND @ResultDate = [ProfileDates].CollectionDate
						THEN CASE 
								WHEN [MandateScores].[Score] = - 1
									THEN 1
								ELSE [MandateScores].[Score] + 1
								END
					ELSE - 1
					END
				,[MandateScores].[PaymentCycle] = @ResultPaymentCycle
				,[MandateScores].[ScoreAdjustedOn] = getdate()
				,[MandateScores].[ScoreAdjustmentReason] = 'Scoring for ' + cast(@ResultPaymentCycle AS VARCHAR)
	WHEN NOT MATCHED BY TARGET
		THEN
			INSERT (
				[MandateId]
				,[ProfileId]
				,DebitRunId
				,[PaymentCycle]
				,[ScoredOn]
				,[Score]
				,[ScoreAdjustedOn]
				,[ScoreAdjustmentReason]
				)
			VALUES (
				@MandateId
				,[ProfileDates].[ProfileId]
				,[ProfileDates].DebitRunId
				,@ResultPaymentCycle
				,getdate()
				,CASE 
					WHEN @ResultType = 'Success'
						AND @ResultDate = [ProfileDates].[CollectionDate]
						THEN 1
					ELSE - 1
					END
				,getdate()
				,'NewProfiles for ' + cast(@ResultPaymentCycle AS VARCHAR)
				);

	-- Fully Covered Set to - 1 in result Payment Cycle
	MERGE [Profiler].[MandateScores] WITH (
			ROWLOCK
			,UPDLOCK
			) AS MandateScores
	USING (
		SELECT [DebitRunId]
			,[ScoredCount]
			,[ProfileDateCount]
			,'FullyCovered' AS [ProfileId]
		FROM (
			SELECT cpd.[DebitRunId]
				,sum(CASE 
						WHEN coalesce(cms.[Score], 0) = - 1
							THEN 1
						ELSE 0
						END) AS ScoredCount
				,count(*) AS ProfileDateCount
			FROM Profiler.ProfileDates cpd WITH (NOLOCK)
			LEFT OUTER JOIN Profiler.MandateScores cms WITH (NOLOCK) ON cms.DebitRunId = cpd.DebitRunId
				AND cms.ProfileId = cpd.ProfileId
				AND cms.MandateId = @MandateId
			WHERE cpd.PaymentCycle BETWEEN @ResultPaymentCycle
					AND @NextPaymentCycle
				AND cpd.CollectionDate <= @NextPaymentDate
				AND cpd.CollectionDate >= @ResultPresentedOnDate
				AND cpd.ProfileId NOT LIKE 'LastDay-ClearsIn%'
			GROUP BY cpd.[DebitRunId]
			) AS MandateProfileDRICoverage
		WHERE ScoredCount = ProfileDateCount
		) AS FullyCoveredSource
		ON MandateScores.DebitRunId = FullyCoveredSource.DebitRunId
			AND MandateScores.ProfileId = FullyCoveredSource.ProfileId
			AND [MandateScores].MandateId = @MandateId
	WHEN MATCHED
		THEN
			UPDATE
			SET [MandateScores].[Score] = - 1
				,[MandateScores].[PaymentCycle] = @ResultPaymentCycle
				,[MandateScores].[ScoreAdjustedOn] = getdate()
				,[MandateScores].[ScoreAdjustmentReason] = 'Scoring for ' + cast(@ResultPaymentCycle AS VARCHAR)
	WHEN NOT MATCHED BY TARGET
		THEN
			INSERT (
				[MandateId]
				,[ProfileId]
				,DebitRunId
				,[PaymentCycle]
				,[ScoredOn]
				,[Score]
				,[ScoreAdjustedOn]
				,[ScoreAdjustmentReason]
				)
			VALUES (
				@MandateId
				,[FullyCoveredSource].[ProfileId]
				,[FullyCoveredSource].DebitRunId
				,@ResultPaymentCycle
				,getdate()
				,- 1
				,getdate()
				,'NewProfiles for ' + cast(@ResultPaymentCycle AS VARCHAR)
				);

	-- Set historic fullycoverd as 0 for success on matching DRI
	UPDATE HistoricFullyCovered
	SET HistoricFullyCovered.Score = 0
	FROM Profiler.MandateScores HistoricFullyCovered WITH (
			ROWLOCK
			,UPDLOCK
			)
	INNER JOIN Profiler.MandateScores OtherMandatesDRIs WITH (NOLOCK) ON OtherMandatesDRIs.MandateId = HistoricFullyCovered.MandateId
		AND OtherMandatesDRIs.ProfileId <> 'FullyCovered'
		AND OtherMandatesDRIs.DebitRunId = HistoricFullyCovered.DebitRunId
		AND OtherMandatesDRIs.Score > 0
		AND OtherMandatesDRIs.ProfileId NOT LIKE 'LastDay-ClearsIn%'
	WHERE HistoricFullyCovered.MandateId = @MandateId
		AND HistoricFullyCovered.ProfileId = 'FullyCovered'
		AND HistoricFullyCovered.Score = - 1
		AND HistoricFullyCovered.PaymentCycle < OtherMandatesDRIs.PaymentCycle

	-- All new fully covered and weekday set other Matching DRI as - 1
	UPDATE OtherMandatesDRIs
	SET OtherMandatesDRIs.Score = - 1
		,OtherMandatesDRIs.PaymentCycle = FailedWeekdays.PaymentCycle
		,OtherMandatesDRIs.ScoreAdjustedOn = FailedWeekdays.ScoreAdjustedOn
		,OtherMandatesDRIs.ScoreAdjustmentReason = FailedWeekdays.ScoreAdjustmentReason
	FROM Profiler.MandateScores OtherMandatesDRIs WITH (
			ROWLOCK
			,UPDLOCK
			)
	INNER JOIN Profiler.MandateScores FailedWeekdays WITH (NOLOCK) ON FailedWeekdays.MandateId = OtherMandatesDRIs.MandateId
		AND FailedWeekdays.ProfileId IN (
			'Weekday'
			,'FullyCovered'
			)
		AND FailedWeekdays.DebitRunId = OtherMandatesDRIs.DebitRunId
		AND FailedWeekdays.ProfileId <> OtherMandatesDRIs.ProfileId
		AND FailedWeekdays.Score = - 1
	WHERE OtherMandatesDRIs.MandateId = @MandateId
		AND OtherMandatesDRIs.PaymentCycle < FailedWeekdays.PaymentCycle
		AND OtherMandatesDRIs.ProfileId NOT LIKE 'LastDay-ClearsIn%'
		AND OtherMandatesDRIs.Score > 0
END
