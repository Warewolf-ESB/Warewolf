SET NOCOUNT ON
GO

MERGE INTO [Profiler].[RecommendationType] AS [Target]
USING (VALUES
  (N'ABTest ExtendInitialTracking')
 ,(N'ABTest GoNoScoresStrategy')
 ,(N'ABTest GoOnScoresStrategy')
 ,(N'ABTest HuntingGo25Once')
 ,(N'ABTest HuntingGo25Twice')
 ,(N'ABTest StandardTracking')
 ,(N'ALGN ABTest GoNoScoresStrategy')
 ,(N'ALGN ABTest GoOnScoresStrategy')
 ,(N'ALGN ABTest HuntingGo25Once')
 ,(N'ALGN ABTest HuntingGo25Twice')
 ,(N'ALGN CapitecDateChange')
 ,(N'ALGN ClearWinner')
 ,(N'ALGN Default')
 ,(N'ALGN FixedExtract')
 ,(N'ALGN GoodDO')
 ,(N'ALGN Hunting')
 ,(N'ALGN NoScores')
 ,(N'ALGN TiedScore')
 ,(N'CapitecDateChange')
 ,(N'ClearWinner')
 ,(N'Compuscan')
 ,(N'Default')
 ,(N'FixedExtract')
 ,(N'GoodDO')
 ,(N'Hunting')
 ,(N'HuntingBadDO')
 ,(N'NA')
 ,(N'NoScores')
 ,(N'TiedScore')
 ,(N'HuntingGo25First')
 ,(N'HuntingGo25Second')
 ,(N'InitialsTracking')
 ,(N'ABTest ExtendTrackingOngoingBook')
 ,(N'ABTest StandardTrackingOngoingBook') 
 ,(N'ALGN ABTest ExtendTrackingOngoingBook')
 ,(N'ALGN ABTest StandardTrackingOngoingBook')
 ,(N'ABTest GoodDOGoDO')
 ,(N'ABTest GoodDOGoDC1')
 ,(N'ALGN ABTest GoodDOGoDO')
 ,(N'ALGN ABTest GoodDOGoDC1')
 ,(N'ABTest DRI15 InitialsTrackingGo15DRI')
 ,(N'ABTest DRI15 InitialsTrackingGo20DRI')
 ,(N'ABTest DRI15 InitialsTrackingGo25DRI')
 ,(N'LearningPhase')
 ,(N'ALGN LearningPhase')
 ,(N'ALGN InitialsTracking')
 ,(N'ALGN HuntingGo25First') 
 ,(N'ALGN HuntingGo25Second')
 ,(N'ABTest GoOnScoresBadDO')
 ,(N'ABTest GoNoScoresBadDO')
 ,(N'ALGN ABTest GoOnScoresBadDO')
 ,(N'ALGN ABTest GoNoScoresBadDO')
 ,(N'BadDO')
 ,(N'ALGN BadDO')
 ,(N'ALGN HuntingBadDO')
 ,(N'GoodPayer')
 ,(N'ALGN GoodPayer')
 ,(N'HuntingLastday')
 ,(N'ALGN HuntingLastday') 
)AS [Source] ([RecommendationTypeId])
ON ([Target].[RecommendationTypeId] = [Source].[RecommendationTypeId])
WHEN MATCHED AND (
	NULLIF([Source].[RecommendationTypeId], [Target].[RecommendationTypeId]) IS NOT NULL OR NULLIF([Target].[RecommendationTypeId], [Source].[RecommendationTypeId]) IS NOT NULL) THEN
 UPDATE SET
  [Target].[RecommendationTypeId] = [Source].[RecommendationTypeId]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([RecommendationTypeId])
 VALUES([Source].[RecommendationTypeId]);

DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [RecommendationType]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[Profiler].[RecommendationType] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO


SET NOCOUNT OFF
GO