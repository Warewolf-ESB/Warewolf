Feature: Scheduler
	In order to schedule workflows
	As a Warewolf user
	I want to setup schedules

@Scheduler	
Scenario: Schedule with history
      Given I have a schedule "ScheduleWithHistory"
	  And "ScheduleWithHistory" executes an Workflow "Hello World" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Enabled"
	  And "ScheduleWithHistory" has a username of "LocalSchedulerAdmin" and a Password of "987Sched#@!"
	  And "ScheduleWithHistory" has a Schedule of
	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
	  | On a schedule | Daily  | 2014/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
	  When the "ScheduleWithHistory" is executed "1" times
	  Then the Schedule task has "NO" error
	  Then the schedule status is "Error"
	  And "ScheduleWithHistory" has "2" row of history	   

@Scheduler
Scenario: Creating task with schedule statud disabled
      Given I have a schedule "Diceroll00"
	  And "Diceroll00" executes an Workflow "Hello World" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Disabled"
	  And "Diceroll00" has a username of "Warewolf Administrators\IntegrationTester" and a Password of "I73573r0"
	  And "Diceroll00" has a Schedule of
	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
	  | On a schedule | "Daily"  | 2014/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
	  When the "Diceroll00" is executed "1" times
	  Then the Schedule task has "An" error

@Scheduler
Scenario: Setting schedule task "At log on"
      Given I have a schedule "Diceroll1"
	  And "Diceroll1" executes an Workflow "Hello World" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Enabled"
	  And "Diceroll1" has a username of "LocalSchedulerAdmin" and a Password of "987Sched#@!"
	  And "Diceroll1" has a Schedule of
	  | ScheduleType | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
	  | At log on    | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
	  Then the Schedule task has "No" error
	  When the "Diceroll1" is executed "1" times
	  Then the schedule status is "Error"
	  And "Diceroll1" has "2" row of history	   
	  

@Scheduler
Scenario: Schedule the task with Incorrect username or password
      Given I have a schedule "Diceroll1"
	  And "Diceroll1" executes an Workflow "Hello World" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Enabled"
	  And "Diceroll1" has a username of "bobthebuilder" and a Password of "I73573r0" 
	  And "Diceroll1" has a Schedule of
	  | ScheduleType | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
	  | At log on    | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
	  Then the Schedule task has "AN" error

@Scheduler
Scenario: Schedule with LocalUser
      Given I have a schedule "LocalUserSchedule"
	  And "LocalUserSchedule" executes an Workflow "Hello World" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Enabled"
	  And "LocalUserSchedule" has a username of "LocalSchedulerAdmin" and a Password of "987Sched#@!"
	  And "LocalUserSchedule" has a Schedule of
	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
	  | On a schedule | Daily  | 2014/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
	  When the "LocalUserSchedule" is executed "1" times
	  Then the Schedule task has "No" error
	  Then the schedule status is "Error"
	  And "LocalUserSchedule" has "2" row of history	   
	  
@Scheduler
Scenario: Schedule with ErrorInDebug
      Given I have a schedule "ScheduleWithError"
	  And "ScheduleWithError" executes an Workflow "moocowimpi" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Enabled"
	  And "ScheduleWithError" has a username of "dev2\IntegrationTester" and a Password of "I73573r0"
	  And "ScheduleWithError" has a Schedule of
	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
	  | On a schedule | Daily    | 2014/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
	  When the "ScheduleWithError" is executed "1" times
	  Then the Schedule task has "An" error
	  Then the schedule status is "Failure"
