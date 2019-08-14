@Scheduler
Feature: Scheduler
	In order to schedule workflows
	As a Warewolf user
	I want to setup schedules
	
Scenario: Schedule with history
      Given I have a schedule "ScheduleWithHistory"
	  And "ScheduleWithHistory" executes an Workflow "Hello World" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Enabled"
	  And "Diceroll00" has a username of "LocalSchedulerAdmin" and a saved password
	  And "ScheduleWithHistory" has a Schedule of
	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |ResourceId |
	  | On a schedule | Daily    | 2020/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2020/01/02 | 15:40:15   |	            |
	  When the "ScheduleWithHistory" is executed "1" times
	  Then the Schedule task has "NO" error
	  Then the schedule status is "Error"
	  And "ScheduleWithHistory" has "2" row of history   

Scenario: Creating task with schedule status is disabled
      Given I have a schedule "Diceroll00"
	  And "Diceroll00" executes an Workflow "Hello World" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Disabled"
	  And "Diceroll00" has a username of "LocalSchedulerAdmin" and a saved password
	  And "Diceroll00" has a Schedule of
	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |ResourceId |
	  | On a schedule | "Daily"  | 2020/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2020/01/02 | 15:40:15   |           |
	  When the "Diceroll00" is executed "1" times
	  Then the Schedule task has "An" error

Scenario: Setting schedule task "At log on"
      Given I have a schedule "Diceroll1"
	  And "Diceroll1" executes an Workflow "Hello World" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Enabled"
	  And "Diceroll1" has a username of "LocalSchedulerAdmin" and a saved password
	  And "Diceroll1" has a Schedule of
	  | ScheduleType | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |ResourceId |
	  | At log on    | 1     | hour          | 1      | hour           | 2020/01/02 | 15:40:15   |           |
	  Then the Schedule task has "No" error
	  When the "Diceroll1" is executed "1" times
	  Then the schedule status is "Error"
	  And "Diceroll1" has "2" row of history	   
	  
Scenario: Schedule the task with Incorrect username or password
      Given I have a schedule "Diceroll1"
	  And "Diceroll1" executes an Workflow "Hello World" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Enabled"
	  And "Diceroll1" has a username of "bobthebuilder" and a saved password 
	  And "Diceroll1" has a Schedule of
	  | ScheduleType | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |ResourceId |
	  | At log on    | 1     | hour          | 1      | hour           | 2020/01/02 | 15:40:15   |           |
	  Then the Schedule task has "AN" error	   
	  
Scenario: Schedule with Error In Debug
      Given I have a schedule "ScheduleWithError"
	  And "ScheduleWithError" executes an Workflow "moocowimpi" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Enabled"
	  And "ScheduleWithError" has a username of "LocalSchedulerAdmin" and a saved password
	  And "ScheduleWithError" has a Schedule of
	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime | ResourceId |
	  | On a schedule | Daily    | 2020/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2020/01/02 | 15:40:15   |            |
	  When the "ScheduleWithError" is executed "1" times
	  Then the Schedule task has "An" error
	  Then the schedule status is "Failure"

Scenario: Schedule Workflow with success
      Given I have a schedule "ScheduleAssignOutput"
	  And "ScheduleAssignOutput" executes an Workflow "AssignOutput" 
	  And task history "Number of history records to load" is "2"
	  And the task status "Status" is "Enabled"
	  And "ScheduleAssignOutput" has a username of "LocalSchedulerAdmin" and a saved password	
	  And "ScheduleAssignOutput" has a Schedule of
	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime | ResourceId                           |
	  | On a schedule | Daily    | 2020/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2020/01/02 | 15:40:15   | e7ea5196-33f7-4e0e-9d66-44bd67528a96 |
	  When the "ScheduleAssignOutput" is executed "1" times
	  Then the Schedule task has "No" error
	  Then the schedule status is "Success"
