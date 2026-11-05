CREATE PROCEDURE [Profiler].[usp_DeleteFutureProfilerRecommendation] (
	@MandateId UNIQUEIDENTIFIER
	,@NextPaymentCycle INT
	)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @RecommendationCount INT;

	SELECT @RecommendationCount = COUNT(*)
	FROM [Profiler].[ProfilerRecommendation] with (nolock)
	WHERE MandateId = @MandateId
		AND ForPaymentCycle >= @NextPaymentCycle;

	IF @RecommendationCount = 1
	BEGIN
		BEGIN TRANSACTION;

		BEGIN TRY
			DELETE rl
			FROM [Profiler].[ProfilerRecommendationLabels] rl
			INNER JOIN [Profiler].[ProfilerRecommendation] pr with (nolock) ON rl.ProfilerRecommendationld = pr.ProfilerRecommendationId
			WHERE pr.MandateId = @MandateId
				AND pr.ForPaymentCycle = @NextPaymentCycle;

			DELETE
			FROM [Profiler].[ProfilerRecommendation]
			WHERE MandateId = @MandateId
				AND ForPaymentCycle = @NextPaymentCycle;

			COMMIT TRANSACTION;
		END TRY

		BEGIN CATCH
			ROLLBACK TRANSACTION;

			THROW;
		END CATCH
	END
END
