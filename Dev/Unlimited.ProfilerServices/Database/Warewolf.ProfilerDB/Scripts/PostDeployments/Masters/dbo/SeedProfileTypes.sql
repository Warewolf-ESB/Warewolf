SET NOCOUNT ON

SET IDENTITY_INSERT [ProfilerTypes] ON

MERGE INTO [ProfilerTypes] AS [Target]
USING (VALUES
  (1,N'ALGN')
 ,(2,N'Compuscan')
 ,(3,N'TUStrategy')
) AS [Source] ([ProfilerTypeId],[Description])
ON ([Target].[ProfilerTypeId] = [Source].[ProfilerTypeId])
WHEN MATCHED AND (
	NULLIF([Source].[Description], [Target].[Description]) IS NOT NULL OR NULLIF([Target].[Description], [Source].[Description]) IS NOT NULL) THEN
 UPDATE SET
  [Target].[Description] = [Source].[Description]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([ProfilerTypeId],[Description])
 VALUES([Source].[ProfilerTypeId],[Source].[Description])
WHEN NOT MATCHED BY SOURCE THEN 
 DELETE;

DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [ProfilerTypes]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[ProfilerTypes] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO



SET IDENTITY_INSERT [ProfilerTypes] OFF
SET NOCOUNT OFF
GO