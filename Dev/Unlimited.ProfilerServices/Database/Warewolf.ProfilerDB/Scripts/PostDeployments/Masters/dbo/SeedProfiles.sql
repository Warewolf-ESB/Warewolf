SET NOCOUNT ON
SET IDENTITY_INSERT [dbo].[profiles] ON
MERGE INTO [dbo].[Profiles] AS [Target]
USING (VALUES
(1,'ThirdLastWorkingDay'),
(2,'ThirdLastFriday'),
(3,'SunPubHolMonAndFriPayThu'),
(4,'SunPubHolFriAndMonPaySat'),
(5,'FifthLastWorkingDay'),
(6,'WeekDay'),
(7,'FirstFriday'),
(8,'PublicHolidayFirstFriday'),
(9,'PublicHolidaySecondFriday'),
(10,'PublicHolidayThirdFriday'),
(11,'PublicHolidayFourthFriday'),
(12,'PublicHolidayLastFriday'),
(13,'LastFriday'),
(14,'LastWorkingDay'),
(15,'PublicHolidaySecondLastFriday'),
(16,'PublicHolidayLastFridayBeforeLastWorkingDay'),
(17,'PubHolFriPayMon'),
(18,'FourthFridayPubHolFriPayMon'),
(19,'PubHolFriPayThu'),
(20,'FirstFridayPubHolFriPayThu'),
(21,'FirstFridayPubHolFriPayMon'),
(22,'PublicHolidayThirdLastFriday'),
(23,'ThirdLastFridayPubHolFriPayMon'),
(24,'PubHolFriPaySat'),
(25,'ThirdLastFridayPubHolFriPaySat'),
(26,'ThirdLastFridayPubHolFriPayThu'),
(27,'ThirdFridayPubHolFriPayThu'),
(28,'ThirdFridayPubHolFriPaySat'),
(29,'ThirdFridayPubHolFriPayMon'),
(30,'ThirdFriday'),
(31,'SunPubHolMonPayTue'),
(32,'SunPubHolMonPaySat'),
(33,'SunPubHolMonPayFri'),
(34,'SunPubHolFriPayThu'),
(35,'SunPubHolFriPaySat'),
(36,'SunPubHolFriPayMon'),
(37,'SunPubHolFriAndMonPayTue'),
(38,'SunPubHolFriAndMonPayThu'),
(39,'SunPaySat'),
(40,'SunPayMon'),
(41,'SunPayFri'),
(42,'Sunday'),
(43,'SecondLastWorkingDay'),
(44,'SecondLastFriday'),
(45,'SecondFridayPubHolFriPayThu'),
(46,'SecondFridayPubHolFriPaySat'),
(47,'SecondFridayPubHolFriPayMon'),
(48,'SecondFriday'),
(49,'SatPubHolMonPayTue'),
(50,'SatPubHolMonPayFri'),
(51,'SatPubHolFriPayThu'),
(52,'SatPubHolFriPayMon'),
(53,'SatPubHolFriAndMonPayTue'),
(54,'SatPubHolFriAndMonPayThu'),
(55,'SatPaySat'),
(56,'SatPayMon'),
(57,'SatPayFri'),
(58,'PubHolWedPayTue'),
(59,'PubHolWedPayThu'),
(60,'PubHolTuePayWed'),
(61,'PubHolTuePayMon'),
(62,'PubHolThuPayWed'),
(63,'PubHolThuPayFri'),
(64,'PubHolSatPayMon'),
(65,'PubHolSatPayFri'),
(66,'PubHolMonPayTue'),
(67,'PubHolMonPaySat'),
(68,'PubHolMonPayFri'),
(69,'PubHolMonAndFriPayTue'),
(70,'PubHolMonAndFriPayThu'),
(71,'PubHolMonAndFriPaySat'),
(72,'PubHolFriAndMonPayTue'),
(73,'PubHolFriAndMonPayThu'),
(74,'PubHolFriAndMonPaySat'),
(75,'LastFridayPubHolFriPayThu'),
(76,'LastFridayPubHolFriPaySat'),
(77,'LastFridayPubHolFriPayMon'),
(78,'LastFridayBeforeLastWorkingDayPubHolFriPayThu'),
(79,'LastFridayBeforeLastWorkingDayPubHolFriPaySat'),
(80,'LastFridayBeforeLastWorkingDayPubHolFriPayMon'),
(81,'LastFridayBeforeLastWorkingDay'),
(82,'FourthLastWorkingDay'),
(83,'FourthFridayPubHolFriPayThu'),
(84,'FourthFridayPubHolFriPaySat'),
(85,'FourthFriday'),
(86,'FirstFridayPubHolFriPaySat'),
(87,'MonPayFri'),
(88,'WeekdayPlus1'),
(89,'WeekdayPlus2'),
(90,'WeekdayPlus3'),
(91,'WeekdayPlus4'),
(92,'WeekdayPlus5'),
(93,'WeekdayPlus6'),
(94,'WeekdayPlus7'),
(95,'WeekdayPlus8'),
(96,'WeekdayPlus9'),
(97,'WeekdayPlus10'),
(98,'WeekdayPlus11'),
(99,'WeekdayPlus12'),
(100,'WeekdayPlus13'),
(101,'WeekdayPlus14'),
(102,'WeekdayPlus15')
) AS [Source] ([ProfileId],[Description])
ON ([Target].[ProfileId] = [Source].[ProfileId])
WHEN MATCHED AND (
	NULLIF([Source].[Description], [Target].[Description]) IS NOT NULL OR NULLIF([Target].[Description], [Source].[Description]) IS NOT NULL) THEN
 UPDATE SET
  [Target].[Description] = [Source].[Description]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([ProfileId],[Description])
 VALUES([Source].[ProfileId],[Source].[Description])
WHEN NOT MATCHED BY SOURCE THEN 
 DELETE;

DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [Profiles]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[Profiles] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO

SET IDENTITY_INSERT [profiles] OFF
SET NOCOUNT OFF
GO
