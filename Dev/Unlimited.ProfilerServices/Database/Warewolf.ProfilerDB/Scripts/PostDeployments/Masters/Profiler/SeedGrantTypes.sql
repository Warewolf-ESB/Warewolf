SET NOCOUNT ON
GO

SET IDENTITY_INSERT [Profiler].[GrantType] ON 
MERGE INTO [Profiler].[GrantType] AS [Target]
USING (VALUES
  (1,'Older Persons Grant')
 ,(2,'Disability Grant')
 ,(3,'Childrens Grant')
) AS [Source] ([GrantTypeId],[GrantName])
ON ([Target].[GrantTypeId] = [Source].[GrantTypeId])
WHEN MATCHED AND (
	NULLIF([Source].[GrantName], [Target].[GrantName]) IS NOT NULL OR NULLIF([Target].[GrantName], [Source].[GrantName]) IS NOT NULL) THEN

 UPDATE SET
  [Target].[GrantName] = [Source].[GrantName]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([GrantTypeId],[GrantName])
 VALUES([Source].[GrantTypeId],[Source].[GrantName]);

DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [[GrantType]]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[Profiler].[[GrantType]] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO


SET NOCOUNT OFF
SET IDENTITY_INSERT [Profiler].[GrantType] OFF 
GO

