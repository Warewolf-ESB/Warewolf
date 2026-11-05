SET NOCOUNT ON
GO

MERGE INTO [Profiler].[Profiles] AS [Target]
USING (VALUES
(N'Fri-PubHolFri-PayMon', N'',0),
(N'Fri-PubHolFri-PaySat', N'',0),
(N'Fri-PubHolFri-PayThu', N'',0),
(N'FriPubHolFriPayThu', N'',0),
(N'Mon-PubHolMon-PayFri', N'',0),
(N'Mon-PubHolMon-PaySat', N'',0),
(N'Mon-PubHolMon-PayTue', N'',0),
(N'Sat-PayFri', N'',0),
(N'Sat-PayMon', N'',0),
(N'Sat-PaySat', N'',0),
(N'Sat-PubHolFri-PayMon', N'',0),
(N'Sat-PubHolFri-PayThu', N'',0),
(N'Sat-PubHolMon-PayFri', N'',0),
(N'Sat-PubHolMon-PayTue', N'',0),
(N'Sat-PubHolSat-PayFri', N'',0),
(N'Sat-PubHolSat-PayMon', N'',0),
(N'Sun-PayFri', N'',0),
(N'Sun-PayMon', N'',0),
(N'Sun-PaySat', N'',0),
(N'Sun-PubHolFri-PayMon', N'',0),
(N'Sun-PubHolFri-PaySat', N'',0),
(N'Sun-PubHolFri-PayThu', N'',0),
(N'Sun-PubHolMon-PayFri', N'',0),
(N'Sun-PubHolMon-PaySat', N'',0),
(N'Sun-PubHolMon-PayTue', N'',0),
(N'Sun-PubHolMonFri-PaySat', N'',0),
(N'Mon-PubHolMonFri-PayThu', N'',0),
(N'Weekday', N'',0),
(N'Weekday-PublicHoliday-PayDayAfter', N'',0),
(N'Weekday-PublicHoliday-PayDayBefore', N'',0),
(N'FullyCovered', N'Have failed on all possible profiles for a DRI',0),
(N'LastDay-ClearsInOneBankDay', N'Tracked 1 banking day into next month to allow clearing of funds in account',1),
(N'LastDay-ClearsInTwoBankDays', N'Tracked 2 banking days into next month to allow clearing of funds in account',1),
(N'LastDay-ClearsInThreeBankDays', N'Tracked 3 banking days into next month to allow clearing of funds in account',1),
(N'LastDay-ClearsInFourBankDays', N'Tracked 4 banking days into next month to allow clearing of funds in account',1),
(N'LastDay-ClearsInFiveBankDays', N'Tracked 5 banking days into next month to allow clearing of funds in account',1),
(N'LastDay-ClearsInSixBankDays', N'Tracked 6 banking days into next month to allow clearing of funds in account',1),
(N'LastDay-ClearsInSevenBankDays', N'Tracked 7 banking days into next month to allow clearing of funds in account',1),
(N'Sat-PubHolThuFri-PayWed', N'',0),
(N'Fri-PubHolThuFri-PayWed', N'',0),
(N'Sun-PubHolThuFri-PayWed', N'',0)
) AS [Source] ([ProfileId],[Description],[IsDeleted])
ON ([Target].[ProfileId] = [Source].[ProfileId])
WHEN MATCHED AND (
	NULLIF([Target].[Description], [Source].[Description]) IS NOT NULL OR 
	NULLIF([Target].[Description], [Source].[Description]) IS NOT NULL OR 
	NULLIF([Target].[IsDeleted], [Source].[IsDeleted]) IS NOT NULL
	
	) THEN
 UPDATE SET
  [Target].[Description] = [Source].[Description],
  [Target].[IsDeleted] = [Source].[IsDeleted]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([ProfileId],[Description],[IsDeleted])
 VALUES([Source].[ProfileId],[Source].[Description],[Source].[IsDeleted])
WHEN NOT MATCHED BY SOURCE THEN 
 DELETE;
GO

DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [Profiler]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[Profiles] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO

SET NOCOUNT OFF
GO
