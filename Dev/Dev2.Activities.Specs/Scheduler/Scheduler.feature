#Feature: Scheduler
#	In order to schedule workflows
#	As a Warewolf user
#	I want to setup schedules
#	
#
#Scenario: Schedule with history
#      Given I have a schedule "ScheduleWithHistory"
#	  And "ScheduleWithHistory" executes an Workflow "Assign" 
#	   And task history "Number of history records to load" is "2"
#	  And the task status "Status" is "Enabled"
#	  And "ScheduleWithHistory" has a username of "UserName" and a Password of "Password"
#	  And "ScheduleWithHistory" has a Schedule of
#	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
#	  | On a schedule | "Daily"  | 2014/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
#	  When the "ScheduleWithHistory" has "Successfull"
#	  Then the Schedule task has "No" error
#	  When the "Diceroll" is executed "2" times
#	  Then the schedule status is "Success"
#	  And "ScheduleWithHistory" has "1" row of history	   
#	  And the history debug output for 'ScheduleWithHistory' for row "1" is 
#		| # |                     |
#		| 1 | [[rec(2).a]] = Test |
#
#Scenario: Schedule the Dice rol workflow and run
#      Given I have a schedule "Diceroll01235"
#	  And "ScheduleWithHistory" executes an Workflow "My Category\Dice Roll" 
#	  And task history "Number of history records to load" is "2"
#	  And the task status "Status" is "Enabled"
#	  And "ScheduleWithHistory" has a username of "IntegrationTester" and a Password of "I73573r0"
#	  And "ScheduleWithHistory" has a Schedule of
#	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
#	  | On a schedule | "Daily"  | 2014/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
#	  When the "ScheduleWithHistory" has "Successfull"
#	  Then the Schedule task has "No" error
#	  When the "Diceroll" is executed "2" times
#	  Then the schedule status is "Success"
#	  And "ScheduleWithHistory" has "2" row of history	   
#	  And the history debug output for 'ScheduleWithHistory' for row "1" is 
#		| # |                    |
#		| 1 | [[DiceRoll]] = int |
#
#Scenario: Creating task with schedule statud disabled
#      Given I have a schedule "Diceroll00"
#	  And "ScheduleWithHistory" executes an Workflow "My Category\Dice Roll" 
#	  And task history "Number of history records to load" is "2"
#	  And the task status "Status" is "Disabled"
#	  And "ScheduleWithHistory" has a username of "IntegrationTester" and a Password of "I73573r0"
#	  And "ScheduleWithHistory" has a Schedule of
#	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
#	  | On a schedule | "Daily"  | 2014/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
#	  When the "ScheduleWithHistory" has "Successfull"
#	  Then the Schedule task has "No" error
#	  When the "Diceroll" is executed "2" times
#	  Then the schedule status is ""
#	  And "ScheduleWithHistory" has "2" row of history	   
#	  And the history debug output for 'ScheduleWithHistory' for row "1" is 
#		| # |                    |
#		| 1 | [[DiceRoll]] = int |
#
#Scenario: Sertting schedule task "At log on"
#      Given I have a schedule "Diceroll1"
#	  And "ScheduleWithHistory" executes an Workflow "My Category\Dice Roll" 
#	  And task history "Number of history records to load" is "2"
#	  And the task status "Status" is "Enabled"
#	  And "ScheduleWithHistory" has a username of "IntegrationTester" and a Password of "I73573r0"
#	  And "ScheduleWithHistory" has a Schedule of
#	  | ScheduleType | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
#	  | At log on    | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
#	  When the "ScheduleWithHistory" has "Successfull"
#	  Then the Schedule task has "No" error
#	  When the "Diceroll" is executed "2" times
#	  When the "ScheduleWithHistory" is executed
#	  Then the schedule status is "Success"
#	  And "ScheduleWithHistory" has "2" row of history	   
#	  And the history debug output for 'ScheduleWithHistory' for row "1" is 
#		| # |                    |
#		| 1 | [[DiceRoll]] = int |
#
#
#Scenario: Schedule the task with Incorrect username or password
#      Given I have a schedule "Diceroll123"
#	  And "ScheduleWithHistory" executes an Workflow "My Category\Dice Roll" 
#	  And task history "Number of history records to load" is "2"
#	  And the task status "Status" is "Enabled"
#	  And "ScheduleWithHistory" has a username of "IntegrationTester" and a Password of "I73573r0"
#	  And "ScheduleWithHistory" has a Schedule of
#	  | ScheduleType  | Interval | StartDate  | StartTime | Recurs | RecursInterval | Delay | DelayInterval | Repeat | RepeatInterval | ExpireDate | ExpireTime |
#	  | On a schedule | "Daily"  | 2014/01/01 | 15:40:44  | 1      | day            | 1     | hour          | 1      | hour           | 2014/01/02 | 15:40:15   |
#	  When the "ScheduleWithHistory" has "Successfull"
#	  Then the Schedule task has "AN" error
#	
#
