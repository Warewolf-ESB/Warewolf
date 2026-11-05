CREATE PROCEDURE [Profiler].[usp_UpdateProfilerRecommendation] @ProfilerRecommendationId INT
	,@CollectionMethod VARCHAR(50)
	,@ServiceType VARCHAR(50)
	,@TrackingDays INT
	,@MandateDRI INT
	,@RecommendationTypes VARCHAR(MAX)
	,@ProfileDateId INT
	,@SourcePaymentId INT
AS
BEGIN
	SET NOCOUNT ON

	UPDATE [Profiler].[ProfilerRecommendation]
	SET [CollectionMethod] = @CollectionMethod
		,[ServiceType] = @ServiceType
		,[TrackingDays] = @TrackingDays
		,[MandateDRI] = @MandateDRI
		,[RecommendationTypes] = @RecommendationTypes
		,[ProfileDateId] = @ProfileDateId
		,[SourcePaymentId] = @SourcePaymentId
	OUTPUT INSERTED.ProfilerRecommendationId
	WHERE [ProfilerRecommendationId] = @ProfilerRecommendationId

	RETURN 0
END
