CREATE VIEW [dbo].[view_ProfileDates]
	AS 
	select 
		[DateName]
		,[DayName]
		,[MonthName]
		--,[DayOfWeek]
		,[DayOfMonth]
		,[MonthOfYear]
		,[YearNo]
		--,[MonthStartDate]
		--,[MonthEndDate]
		,[PubHolFlag]
		,[WeekDayFlag]
		--,[WeekStartDate]
		--,[WeekEndDate]
		,[DebitRunID]
		,[DebitRunIDDayName]
		,[DebitRunIDDate]
		,[DebitRunIDMonthName]
		,[LUCCollectionProfile]
	
	from
		(select * from [Profiler].view_FriPubHolFriPayMon
		union all
		select * from [Profiler].view_FriPubHolFriPayThu
		union all
		select * from [Profiler].view_MonPubHolMonPayFri
		union all
		select * from [Profiler].view_MonPubHolMonPaySat
		union all
		select * from [Profiler].view_MonPubHolMonPayTue
		union all
		select * from [Profiler].view_SatPayFri
		union all
		select * from [Profiler].view_SatPayMon
		union all
		select * from [Profiler].view_SatPaySat
		union all
		select * from [Profiler].view_SatPubHolFriPayMon
		union all
		select * from [Profiler].view_SatPubHolFriPayThu
		union all
		select * from [Profiler].view_SatPubHolMonPayFri
		union all
		select * from [Profiler].view_SatPubHolMonPayTue
		union all
		select * from [Profiler].view_SatPubHolSatPayFri
		union all
		select * from [Profiler].view_SatPubHolSatPayMon
		union all
		select * from [Profiler].view_SunPayFri
		union all
		select * from [Profiler].view_SunPayMon
		union all
		select * from [Profiler].view_SunPaySat
		union all
		select * from [Profiler].view_SunPubHolFriPayMon
		union all
		select * from [Profiler].view_SunPubHolFriPaySat
		union all
		select * from [Profiler].view_SunPubHolFriPayThu
		union all
		select * from [Profiler].view_SunPubHolMonPayFri
		union all
		select * from [Profiler].view_SunPubHolMonPaySat
		union all
		select * from [Profiler].view_SunPubHolMonPayTue
		union all
		select * from [Profiler].view_WeekDay
		union all
		select * from [Profiler].view_Weekday_FirstFriday
		union all
		select * from [Profiler].view_Weekday_FourthFriday
		union all
		select * from [Profiler].view_Weekday_LastFriday
		union all
		select * from [Profiler].view_Weekday_SecondFriday
		union all
		select * from [Profiler].view_Weekday_ThirdFriday
		union all
		select * from [Profiler].view_WeekdayPublicHolidayPayDayAfter
		union all
		select * from [Profiler].view_WeekdayPublicHolidayPayDayBefore
		union all
		select * from [Profiler].view_FriPubHolFriPaySat
		union all
		select * from [Profiler].view_SunPubHolMonFriPaySat
		union all
		select * from [Profiler].view_CrossApplyLastDayToProfiles
		union all
		select * from [Profiler].[view_CrossApplyLastWorkingDayProfiles]
		union all
		select * from [Profiler].[view_LastDayClearsInBankDays]
		union all
		select * from [Profiler].[view_WeekDay_OlderPersonGrant]
		union all
		select * from [Profiler].[view_WeekDay_DisabilityGrant]
		union all
		select * from [Profiler].[view_WeekDay_ChildrensGrant]
		union all
		select * from [Profiler].[view_MonPubHolMonFriPayThu]
		union all
		select * from [Profiler].[view_SatPubHolThuFriPayWed]
		union all
		select * from [Profiler].[view_FriPubHolThuFriPayWed]
		union all
		select * from [Profiler].[view_SunPubHolThuFriPayWed]
) 
as ProfileDates
where [MonthName] = [DebitRunIDMonthName] --Rule to prevent dates into other months
or [LUCCollectionProfile] like 'LastDay-ClearsIn%' --Rule Allow tracking into next month