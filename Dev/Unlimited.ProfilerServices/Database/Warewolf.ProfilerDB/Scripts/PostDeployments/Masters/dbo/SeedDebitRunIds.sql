SET NOCOUNT ON

MERGE INTO [dbo].[debitrunids] AS [Target]
USING (VALUES
  (1,N'D1',0)
 ,(2,N'D2',0)
 ,(3,N'D3',0)
 ,(4,N'D4',0)
 ,(5,N'D5',0)
 ,(6,N'D6',0)
 ,(7,N'D7',0)
 ,(8,N'D8',0)
 ,(9,N'D9',0)
 ,(10,N'D10',0)
 ,(11,N'D11',0)
 ,(12,N'D12',0)
 ,(13,N'D13',0)
 ,(14,N'D14',0)
 ,(15,N'D15',0)
 ,(16,N'D16',0)
 ,(17,N'D17',0)
 ,(18,N'D18',0)
 ,(19,N'D19',0)
 ,(20,N'D20',0)
 ,(21,N'D21',0)
 ,(22,N'D22',0)
 ,(23,N'D23',0)
 ,(24,N'D24',0)
 ,(25,N'D25',0)
 ,(26,N'D26',0)
 ,(27,N'D27',0)
 ,(28,N'D28',0)
 ,(29,N'D29',0)
 ,(30,N'D30',0)
 ,(31,N'DLastDay',0)
 ,(32,N'DLastFriday',0)
 ,(33,N'DThirdFriday',0)
 ,(34,N'DFourthFriday',0)
 ,(35,N'DSecondFriday',0)
 ,(36,N'DLastFridayBeforeLastWorkingDay',0)
 ,(37,N'DFirstFriday',0)
 ,(42,N'DSecondLastWorkingDay',0)
 ,(43,N'DThirdLastWorkingDay',0)
 ,(44,N'DFourthLastWorkingDay',0)
 ,(45,N'DFifthLastWorkingDay',0)
) AS [Source] ([DebitRunId],[Description],[IsDeleted])
ON ([Target].[DebitRunId] = [Source].[DebitRunId])
WHEN MATCHED AND (
	NULLIF([Source].[Description], [Target].[Description]) IS NOT NULL OR NULLIF([Target].[Description], [Source].[Description]) IS NOT NULL OR 
	NULLIF([Source].[IsDeleted], [Target].[IsDeleted]) IS NOT NULL OR NULLIF([Target].[IsDeleted], [Source].[IsDeleted]) IS NOT NULL) THEN
 UPDATE SET
  [Target].[Description] = [Source].[Description], 
  [Target].[IsDeleted] = [Source].[IsDeleted]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([DebitRunId],[Description],[IsDeleted])
 VALUES([Source].[DebitRunId],[Source].[Description],[Source].[IsDeleted])
WHEN NOT MATCHED BY SOURCE THEN 
 DELETE;

DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [debitrunids]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[debitrunids] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO


SET NOCOUNT OFF
GO