SET NOCOUNT ON
GO

SET IDENTITY_INSERT [Profiler].[GrantDates] ON 
MERGE INTO [Profiler].[GrantDates] AS [Target]
USING (VALUES
  (1,'2025-01-03',202501,1)
 ,(2,'2025-01-06',202501,2)
 ,(3,'2025-01-07',202501,3)
 ,(4,'2025-02-04',202502,1)
 ,(5,'2025-02-05',202502,2)
 ,(6,'2025-02-06',202502,3)
 ,(7,'2025-03-04',202503,1)
 ,(8,'2025-03-05',202503,2)
 ,(9,'2025-03-06',202503,3)
 ,(10,'2025-04-02',202504,1)
 ,(11,'2025-04-03',202504,2)
 ,(12,'2025-04-04',202504,3)
 ,(13,'2025-05-06',202505,1)
 ,(14,'2025-05-07',202505,2)
 ,(15,'2025-05-08',202505,3)
 ,(16,'2025-06-03',202506,1)
 ,(17,'2025-06-04',202506,2)
 ,(18,'2025-06-05',202506,3)
 ,(19,'2025-07-02',202507,1)
 ,(20,'2025-07-03',202507,2)
 ,(21,'2025-07-04',202507,3)
 ,(22,'2025-08-05',202508,1)
 ,(23,'2025-08-06',202508,2)
 ,(24,'2025-08-07',202508,3)
 ,(25,'2025-09-02',202509,1)
 ,(26,'2025-09-03',202509,2)
 ,(27,'2025-09-04',202509,3)
 ,(28,'2025-10-02',202510,1)
 ,(29,'2025-10-03',202510,2)
 ,(30,'2025-10-06',202510,3)
 ,(31,'2025-11-04',202511,1)
 ,(32,'2025-11-05',202511,2)
 ,(33,'2025-11-06',202511,3)
 ,(34,'2025-12-02',202512,1)
 ,(35,'2025-12-03',202512,2)
 ,(36,'2025-12-04',202512,3)
 ,(37,'2026-01-06',202601,1)
 ,(38,'2026-01-07',202601,2)
 ,(39,'2026-01-08',202601,3)
 ,(40,'2026-02-03',202602,1)
 ,(41,'2026-02-04',202602,2)
 ,(42,'2026-02-05',202602,3)
 ,(43,'2026-03-03',202603,1)
 ,(44,'2026-03-04',202603,2)
 ,(45,'2026-03-05',202603,3)
 ,(46,'2026-04-02',202604,1)
 ,(47,'2026-04-07',202604,2)
 ,(48,'2026-04-08',202604,3)
 ,(49,'2026-05-05',202605,1)
 ,(50,'2026-05-06',202605,2)
 ,(51,'2026-05-07',202605,3)
 ,(52,'2026-06-02',202606,1)
 ,(53,'2026-06-03',202606,2)
 ,(54,'2026-06-04',202606,3)
 ,(55,'2026-07-02',202607,1)
 ,(56,'2026-07-03',202607,2)
 ,(57,'2026-07-06',202607,3)
 ,(58,'2026-08-04',202608,1)
 ,(59,'2026-08-05',202608,2)
 ,(60,'2026-08-06',202608,3)
 ,(61,'2026-09-02',202609,1)
 ,(62,'2026-09-03',202609,2)
 ,(63,'2026-09-04',202609,3)
 ,(64,'2026-10-02',202610,1)
 ,(65,'2026-10-05',202610,2)
 ,(66,'2026-10-06',202610,3)
 ,(67,'2026-11-03',202611,1)
 ,(68,'2026-11-03',202611,2)
 ,(69,'2026-11-05',202611,3)
 ,(70,'2026-12-02',202612,1)
 ,(71,'2026-12-03',202612,2)
 ,(72,'2026-12-04',202612,3)
 ,(73,'2027-01-05',202701,1)
 ,(74,'2027-01-06',202701,2)
 ,(75,'2027-01-07',202701,3)
 ,(76,'2027-02-02',202702,1)
 ,(77,'2027-02-03',202702,2)
 ,(78,'2027-02-04',202702,3)
 ,(79,'2027-03-02',202703,1)
 ,(80,'2027-03-03',202703,2)
 ,(81,'2027-03-04',202703,3)
) AS [Source] ([GrantDateId],[Date],[PaymentCycle],[GrantTypeId])
ON ([Target].[GrantDateId] = [Source].[GrantDateId])
WHEN MATCHED AND (
	NULLIF([Source].[Date], [Target].[Date]) IS NOT NULL OR NULLIF([Target].[Date], [Source].[Date]) IS NOT NULL OR 
	NULLIF([Source].[PaymentCycle], [Target].[PaymentCycle]) IS NOT NULL OR NULLIF([Target].[PaymentCycle], [Source].[PaymentCycle]) IS NOT NULL OR
	NULLIF([Source].[GrantTypeId], [Target].[GrantTypeId]) IS NOT NULL OR NULLIF([Target].[GrantTypeId], [Source].[GrantTypeId]) IS NOT NULL) THEN

 UPDATE SET
  [Target].[Date] = [Source].[Date], 
  [Target].[PaymentCycle] = [Source].[PaymentCycle], 
  [Target].[GrantTypeId] = [Source].[GrantTypeId]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([GrantDateId],[Date],[PaymentCycle],[GrantTypeId])
 VALUES([Source].[GrantDateId],[Source].[Date],[Source].[PaymentCycle],[Source].[GrantTypeId]);

DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [GrantDates]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[Profiler].[GrantDates] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO

SET NOCOUNT OFF
SET IDENTITY_INSERT [Profiler].[GrantDates] OFF 
GO
