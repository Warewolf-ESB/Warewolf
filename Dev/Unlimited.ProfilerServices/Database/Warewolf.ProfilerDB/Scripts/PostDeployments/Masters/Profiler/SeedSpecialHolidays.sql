
SET NOCOUNT ON

MERGE INTO [Profiler].[SpecialHolidays] AS [Target]
USING (VALUES
  ('2023-12-15',N'Rugby World Cup Victory Day','Rugby World Cup Victory Day')
 ,('2024-05-29',N'Election Day','South African National Election Day')
 ,('2026-11-04',N'Election Day','Local Government Election Day')
) AS [Source] ([Date],[HolidayName],[Description])
ON ([Target].[Date] = [Source].[Date])
WHEN MATCHED AND (
	NULLIF([Source].[HolidayName], [Target].[HolidayName]) IS NOT NULL OR NULLIF([Target].[HolidayName], [Source].[HolidayName]) IS NOT NULL OR 
	NULLIF([Source].[Description], [Target].[Description]) IS NOT NULL OR NULLIF([Target].[Description], [Source].[Description]) IS NOT NULL) THEN
 UPDATE SET
  [Target].[HolidayName] = [Source].[HolidayName], 
  [Target].[Description] = [Source].[Description]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([Date],[HolidayName],[Description])
 VALUES([Source].[Date],[Source].[HolidayName],[Source].[Description]);

DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [holiday]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[Profiler].[SpecialHolidays] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO


SET NOCOUNT OFF
GO

