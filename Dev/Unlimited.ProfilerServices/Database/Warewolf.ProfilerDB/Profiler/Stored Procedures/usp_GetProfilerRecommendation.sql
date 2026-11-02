CREATE PROCEDURE [Profiler].[usp_GetProfilerRecommendation] @MandateId UNIQUEIDENTIFIER
	,@ForPaymentCycle INT
AS
BEGIN
	SET NOCOUNT ON

	SELECT TOP 1 [PR].[ProfilerRecommendationId]
		,[PR].[MandateId]
		,[PR].[ForPaymentCycle]
		,[PD].[CollectionDate]
		,[PR].[CollectionMethod]
		,[PR].[ServiceType]
		,[PR].[TrackingDays]
		,[PR].[MandateDRI]
		,[PR].[RecommendationTypes]
		,[PR].[ProfileDateId]
		,[PD].[ProfileId]
		,[PR].SourcePaymentId
		,[PR].[InsertedOn]
	FROM [Profiler].[ProfilerRecommendation] PR WITH (NOLOCK)
	INNER JOIN [Profiler].[ProfileDates] PD WITH (NOLOCK) ON [PD].[ProfileDateId] = [PR].[ProfileDateId]
	WHERE [PR].[MandateId] = @MandateId
		AND [PR].[ForPaymentCycle] = @ForPaymentCycle
	ORDER BY [PR].[ProfilerRecommendationId] DESC
END

RETURN 0
