CREATE PROCEDURE [Profiler].[usp_InsertProfilerRecommendation] 
	@MandateId UNIQUEIDENTIFIER
	,@ForPaymentCycle INT
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

	DECLARE @ProfilerRecommendationId INT

	SELECT TOP 1 @ProfilerRecommendationId = ProfilerRecommendationId
	FROM [Profiler].[ProfilerRecommendation] WITH (NOLOCK)
	WHERE [MandateId] = @MandateId
		AND [ForPaymentCycle] = @ForPaymentCycle

	IF @ProfilerRecommendationId IS NOT NULL
	BEGIN
		SELECT @ProfilerRecommendationId AS ProfilerRecommendationId
			,1 AS [AlreadyExists]
	END
	ELSE
	BEGIN
		INSERT INTO [Profiler].[ProfilerRecommendation] (
			[MandateId]
			,[ForPaymentCycle]
			,[CollectionMethod]
			,[ServiceType]
			,[TrackingDays]
			,[MandateDRI]
			,[RecommendationTypes]
			,[ProfileDateId]
			,[SourcePaymentId]
			,[InsertedOn]
			)
		OUTPUT INSERTED.ProfilerRecommendationId
			,0 AS [AlreadyExists]
		VALUES (
			@MandateId
			,@ForPaymentCycle
			,@CollectionMethod
			,@ServiceType
			,@TrackingDays
			,@MandateDRI
			,@RecommendationTypes
			,@ProfileDateId
			,@SourcePaymentId
			,GETDATE()
			)
	END
END

RETURN 0
