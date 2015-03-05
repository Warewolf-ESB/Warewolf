Feature: Settings Tab
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@Settings
Scenario: Settings Opened
	Given I have settings tab opened
	And server selected as "localhost (Connected)"
	And Server edit is "Disabled"
	And server connection is "Disabled"
	And Security is "Selected"
	And Logging is "Unselected"
	And Server Permissions is "Visible"
	| Windows Group          | Edit Group | Deploy To | Deploy From | Administrator | View | Execute | Contribute | Delete Row | Row      |
	| Warewolf Administrator | Disabeled  | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   | Disabled |
	| Public                 | Disabeled  | ""        | ""          | ""            | ""   | ""      | ""         | ""         | Enabled  |
	|                        | Enabled    |           |             |               |      |         |            |            | Enabled  |
	And Resource Permissions is "Visible"
	| Resources | REdit   | Windows Group | WEdit   | View | Execute | Contribute |
	|           | Enabled |               | Enabled |      |         |            |
	And Save is "Disabled"


Scenario: Selecting Admin rights for public
	Given I have settings tab opened
	And server selected as "localhost (Connected)"
	And Security is "Selected"
	And Save is "Disabled"
	And Server Permissions is "Visible"
	And Resource Permissions is "Visible"
	When i select server "Public" as "Administrator"
	| Windows Group          | Edit Group | Deploy To | Deploy From | Administrator | View | Execute | Contribute | Delete Row | Row      |
	| Warewolf Administrator | Disabeled  | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   | Disabled |
	| Public                 | Disabeled  | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   | Enabled  |
	|                        | Enabled    |           |             |               |      |         |            |            | Enabled  |
	Then Save is "Enabled"
	When I save the settings
	Then settings saved "Successfull"
	And the "Settings" has validation error "False"


Scenario: Selecting Resource Permissions
	Given I have settings tab opened
	And server selected as "localhost (Connected)"
	And Save is "Disabled"
	And Security is "Selected"
	And Logging is "Unselected"
	And Server Permissions is "Visible"
	| Windows Group          | Edit Group | Deploy To | Deploy From | Administrator | View | Execute | Contribute | Delete Row | Row      |
	| Warewolf Administrator | Disabeled  | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   | Disabled |
	| Public                 | Disabeled  | ""        | ""          | ""            | ""   | ""      | ""         | ""         | Enabled  |
	|                        | Enabled    |           |             |               |      |         |            |            | Enabled  |
	And Resource Permissions is "Visible"
	When i select resource "Resource Permissions" 
	| Resources                       | REdit   | Windows Group | WEdit   | View | Execute | Contribute |
	| WORKFLOWS\My Category\Dice Roll | Enabled | Public        | Enabled | Yes  | Yes     | Yes        |
	|                                 | Enabled |               | Enabled |      |         |            |  
    Then Save is "Enabled"
	When I save the settings
	Then settings saved "Successfull"
	And the "Settings" has validation error "False"





