Feature: Scheduler
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Scheduler
Scenario: Scheduler Tab
	Given I have Scheduler tab opened
	And 'New' is "Enabled"
	And 'Save' is "Disabled"
	And 'Delete' is "Disabled"
	And server is selected as "localhost"
	And 'Edit" is 'Disabled"
	And 'Connect' is "Disabled"
	When I click on 'New'
	Then the settings as
	| Name        | Status  | Next Execution |
	| New Task 1* | Enabled | Int            |
	And 'Save' is "Enabled"
	And 'Delete' is "Enabled"
	And Name of the task as "New Task1"
	| Name       | Status Selected | Workflow | Edit     | Run Task as soon as | History | Edit Trigger |
	| New Task 1 | Enabled         |          | Disabled |                     |         | Enabled      |
	And username is as ""
	And Password is as ""


Scenario: Saving New Schedule
	Given I have Scheduler tab opened
	When I click on 'New'
	Then the 'New Task1' is created
	| Name        | Status  | Next Execution |
	| New Task 1* | Enabled | Int            |
	And 'Save' is "Enabled"
	And 'Delete' is "Enabled"
	Then the settings as
	| Name      | Status Selected | Workflow              | Edit    | Run Task as soon as | History |Edit Trigger |
	| Dice Roll | Enabled         | My Category\Dice Roll | Enabled |                     |         |Enabled      |
	And username is as "IntegrationTester"sdsd
	And Password is as "I73573r0"
    When I save the Task
	Then Task 'Dice Roll' is saved 
	And Save is "Disabled"


Scenario: Deleting a schedule in Scheduler
	Given I have Scheduler tab opened
	And the saved tasks are
	| Name                  | Status  | Next Execution |
	| Dice Roll             | Enabled | Int            |
	| Double Roll and Check | Enabled | Int            |
	And 'Save' is "Disabled"
	And 'Delete' is "Enabled"
	And settings as
	| Name      | Status Selected | Workflow              | Edit    | Run Task as soon as | History |Edit Trigger |
	| Dice Roll | Enabled         | My Category\Dice Roll | Enabled |                     |         |Enabled      |
	And username is as "IntegrationTester"sdsd
	And Password is as ""
    When I Delete the Schedule
	Then Task 'Dice Roll' is Deleted 
	And Save is "Disabled"
	| Name                  | Status  | Next Execution |
	| Double Roll and Check | Enabled | Int            |
	And Name of the task as "New Task1"
	| Name      | Status Selected | Workflow              | Edit    | Run Task as soon as | History |Edit Trigger |
	| Dice Roll | Enabled         | My Category\Dice Roll | Enabled |                     |         |Enabled      |



Scenario: Selected task is showing correct settings
	Given I have Scheduler tab opened
	And the saved tasks are
	| Name                  | Status  | Next Execution |
	| Dice Roll             | Enabled | Int            |
	| Double Roll and Check | Enabled | Int            |
	And settings as
	| Name      | Status Selected | Workflow              | Edit    | Run Task as soon as | History |Edit Trigger |
	| Dice Roll | Enabled         | My Category\Dice Roll | Enabled |                     |         |Enabled      |
	And 'Dice Roll' task is selected 
	When I select 'Double Roll and Check' task
	Then settings as
	| Name      | Status Selected | Workflow                          | Edit    | Run Task as soon as | History |Edit Trigger |
	| Dice Roll | Enabled         | My Category\Double Roll and Check | Enabled |                     |         |Enabled      |



Scenario: Editing scheduled task is prompting to save
	Given I have Scheduler tab opened
	And the saved tasks are
	| Name                  | Status  | Next Execution |
	| Dice Roll             | Enabled | Int            |
	| Double Roll and Check | Enabled | Int            |
	And 'Dice Roll' task is selected 
	And settings as
	| Name      | Status Selected | Workflow              | Edit    | Run Task as soon as | History |Edit Trigger |
	| Dice Roll | Enabled         | My Category\Dice Roll | Enabled |                     |         |Enabled      |
	When I edit the settings as
	| Name      | Status Selected | Workflow              | Edit    | Run Task as soon as | History | Edit Trigger |
	| Dice Roll | Disabled        | My Category\Dice Roll | Enabled |                     |         | Enabled      |
	Then 'save' is 'Enabled'
	When I save the Task
	Then 'Dice Roll' is saved
	And the saved tasks are
	| Name                  | Status   | Next Execution |
	| Dice Roll             | Disabled | Int            |
	| Double Roll and Check | Enabled  | Int            |






























