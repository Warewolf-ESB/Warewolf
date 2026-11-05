CREATE PROCEDURE [Profiler].[usp_AddNewDebitRunIdProfileAsTied] @MandateId UNIQUEIDENTIFIER
	,@NewDebitRunId INT
	,@PaymentCycle INT
	,@ScoreAdjustmentReason VARCHAR(200)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @ProcessResult AS VARCHAR(50)
	DECLARE @TransactionDate AS DATETIME = GETDATE()
	DECLARE @NewProfileId VARCHAR(50)
	DECLARE @NewDRITotalScore INT = 0
	DECLARE @HighestOtherDRIScore INT = 0

	SELECT TOP 1 @NewProfileId = ProfileId
	FROM [Profiler].[ProfileDates] WITH (NOLOCK)
	WHERE [DebitRunId] = @NewDebitRunId
		AND [PaymentCycle] = @PaymentCycle
	ORDER BY [CollectionDate] ASC

	SELECT @NewDRITotalScore = sum(score)
	FROM [Profiler].[MandateScores]
	WHERE [MandateId] = @MandateId
		AND [Score] > 0
		AND [DebitRunId] = @NewDebitRunId
	GROUP BY [DebitRunId]

	IF NOT EXISTS (
			/*Check to ensure the DRI and Profile has not failed previously*/
			SELECT 1
			FROM [Profiler].[MandateScores] WITH (NOLOCK)
			WHERE [MandateId] = @MandateId
				AND [DebitRunId] = @NewDebitRunId
				AND (
					[ProfileId] = @NewProfileId
					OR [ProfileId] = 'Weekday'
					)
				AND [Score] = - 1
			)
	BEGIN /*To add new DRI check if there are any other positive scores*/
		SET @HighestOtherDRIScore = (
				SELECT TOP 1 sum([Score]) AS TotalPositiveScore
				FROM [Profiler].[MandateScores]
				INNER JOIN [Profiler].[DebitRunIds] ON [DebitRunIds].[DebitRunId] = [MandateScores].[DebitRunId]
					AND [DebitRunIds].[IsDeleted] = 0
				WHERE [MandateId] = @MandateId
					AND [Score] > 0
					AND [MandateScores].[DebitRunId] <> @NewDebitRunId
				GROUP BY [MandateScores].[DebitRunId]
				ORDER BY 1 DESC
				)

		IF coalesce(@HighestOtherDRIScore, 0) > @NewDRITotalScore
		BEGIN
			MERGE [Profiler].[MandateScores] WITH (
					ROWLOCK
					,UPDLOCK
					) AS MandateScores
			USING (
				VALUES (
					@NewDebitRunId
					,@NewProfileId
					,@MandateId
					)
				) AS [NewDRIProfile](DebitRunId, ProfileId, MandateId)
				ON [MandateScores].[ProfileId] = [NewDRIProfile].[ProfileId]
					AND [MandateScores].[DebitRunId] = [NewDRIProfile].[DebitRunId]
					AND [MandateScores].[MandateId] = [NewDRIProfile].[MandateId]
			WHEN MATCHED
				THEN
					UPDATE
					SET [MandateScores].[Score] = [MandateScores].[Score] + (@HighestOtherDRIScore - @NewDRITotalScore)
						,[MandateScores].[PaymentCycle] = @PaymentCycle
						,[MandateScores].[ScoreAdjustedOn] = getdate()
						,[MandateScores].[ScoreAdjustmentReason] = @ScoreAdjustmentReason
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
						,[NewDRIProfile].[ProfileId]
						,[NewDRIProfile].[DebitRunId]
						,@PaymentCycle
						,getdate()
						,@HighestOtherDRIScore - @NewDRITotalScore
						,getdate()
						,@ScoreAdjustmentReason
						);
		END

		SET @ProcessResult = 'Tied DRI'
	END
	ELSE /*DRI not valid, dont add DRI*/
	BEGIN
		SET @ProcessResult = 'Skipped'
	END

	SELECT @ProcessResult AS ProcessResult
END

RETURN 0
