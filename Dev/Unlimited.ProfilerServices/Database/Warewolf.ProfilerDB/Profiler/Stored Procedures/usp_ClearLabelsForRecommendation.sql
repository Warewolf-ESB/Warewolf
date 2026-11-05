CREATE PROCEDURE [Profiler].[usp_ClearLabelsForRecommendation] (@ProfilerRecommendationId INT)
AS
BEGIN
	
	SET NOCOUNT ON;

	IF @ProfilerRecommendationId IS NOT NULL
	BEGIN
		DELETE
		FROM Profiler.ProfilerRecommendationLabels
		WHERE ProfilerRecommendationld = @ProfilerRecommendationId
	END

	
END
