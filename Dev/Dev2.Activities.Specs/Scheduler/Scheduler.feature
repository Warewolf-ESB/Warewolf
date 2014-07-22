Feature: Scheduler
	In order to schedule workflows
	As a Warewolf user
	I want to setup schedules
	

#Scenario: Schedule with history
#      Given I have a schedule "ScheduleWithHistory"
#	  And "ScheduleWithHistory" executes an Workflow "Assign" 
#	  And "ScheduleWithHistory" has a username of "UserName" and a Password of "Password"
#	  And "ScheduleWithHistory" has a Schedule of
#	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
#	  | On a schedule | "Daily"  | 2014/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
#	  When the "ScheduleWithHistory" is executed
#	  Then the schedule status is "Success"
#	  And "ScheduleWithHistory" has "1" row of history	   
#	  And the history debug output for 'ScheduleWithHistory' for row "1" is 
#		| # |                     |
#		| 1 | [[rec(2).a]] = Test |