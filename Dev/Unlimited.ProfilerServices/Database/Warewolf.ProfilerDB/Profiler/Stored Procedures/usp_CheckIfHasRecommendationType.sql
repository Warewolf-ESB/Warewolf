CREATE PROCEDURE [Profiler].[usp_CheckIfHasRecommendationType] 
	@MandateId UNIQUEIDENTIFIER
	,@RecommendationTypes VARCHAR(MAX)
	,@PaymentCycleCountlimit INT = 24
AS
BEGIN
	SET NOCOUNT ON

	IF @PaymentCycleCountlimit IS NULL
	BEGIN
		SET @PaymentCycleCountlimit = 24
	END

	DECLARE @tblRecommendationTypes TABLE (RecommendationTypes VARCHAR(200))
	DECLARE @tblExistingRecomendationTypes TABLE (
		PaymentCycle INT
		,RecommendationType VARCHAR(200)
		)
	DECLARE @tblProfilerRecommendations TABLE (
		ProfilerRecommendationId INT
		,ForPaymentCycle INT
		,RecommendationTypes VARCHAR(200)
		)

	INSERT INTO @tblProfilerRecommendations (
		ProfilerRecommendationId
		,ForPaymentCycle
		,RecommendationTypes
		)
	SELECT TOP (@PaymentCycleCountlimit) pr.ProfilerRecommendationId
		,pr.ForPaymentCycle
		,pr.RecommendationTypes
	FROM Profiler.ProfilerRecommendation pr WITH (NOLOCK)
	WHERE pr.MandateId = @MandateId
	ORDER BY pr.ForPaymentCycle DESC

	INSERT INTO @tblRecommendationTypes (RecommendationTypes)
	SELECT VALUE
	FROM STRING_SPLIT(@RecommendationTypes, ',')

	IF EXISTS (
			SELECT TOP 1 1
			FROM @tblProfilerRecommendations pr
			INNER JOIN @tblRecommendationTypes AS t ON pr.RecommendationTypes = t.RecommendationTypes
			)
	BEGIN
		INSERT INTO @tblExistingRecomendationTypes (
			PaymentCycle
			,RecommendationType
			)
		SELECT ForPaymentCycle
			,pr.RecommendationTypes
		FROM @tblProfilerRecommendations pr
		INNER JOIN @tblRecommendationTypes AS t ON pr.RecommendationTypes = t.RecommendationTypes
	END

	IF EXISTS (
			SELECT TOP 1 1
			FROM Profiler.ProfilerRecommendationLabels pl WITH (NOLOCK)
			INNER JOIN @tblProfilerRecommendations pr ON pl.ProfilerRecommendationld = pr.ProfilerRecommendationId
			INNER JOIN @tblRecommendationTypes AS t ON pl.Label = t.RecommendationTypes
			)
	BEGIN
		INSERT INTO @tblExistingRecomendationTypes (
			PaymentCycle
			,RecommendationType
			)
		SELECT ForPaymentCycle
			,pl.Label
		FROM Profiler.ProfilerRecommendationLabels pl WITH (NOLOCK)
		INNER JOIN @tblProfilerRecommendations pr ON pl.ProfilerRecommendationld = pr.ProfilerRecommendationId
		INNER JOIN @tblRecommendationTypes AS t ON pl.Label = t.RecommendationTypes
	END

	IF EXISTS (
			SELECT TOP 1 1
			FROM @tblExistingRecomendationTypes
			)
	BEGIN
		SELECT TOP 1 'True' AS 'Exists'
			,RecommendationType AS LatestRecommendationType
		FROM @tblExistingRecomendationTypes
		ORDER BY PaymentCycle DESC
	END
	ELSE
	BEGIN
		SELECT 'False' AS 'Exists'
			,NULL AS LatestRecommendationType
	END
END
