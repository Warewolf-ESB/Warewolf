@Settings
Feature: Settings Tab
	In order to manage various server settings
	As a Warewolf User
	I want to be shown and allowed to edit the server settings


Scenario: Settings Opened
	Given I have settings tab opened
	And selected server is "localhost"
	And Security is selected
	And Logging is not selected
	And Server Permissions are "Visible" as
	| Windows Group          | Can Edit Windows Group | Deploy To | Deploy From | Administrator | View | Execute | Contribute | Delete Row |
	| Warewolf Administrator | No                     | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   |
	| Public                 | No                     | No        | No          | No            | No   | No      | No         | Enabled    |
	|                        | Yes                    |           |             |               |      |         |            |            |
	And Resource Permissions are "Visible" as
	| Resource | Can Edit Resource | Windows Group | Can Edit Windows Group | View | Execute | Contribute |
	|          | Yes               |               | Yes                    |      |         |            |
	And "Save" is "Disabled"


Scenario: Selecting Admin rights for public
	Given I have settings tab opened
	And selected server as "localhost"
	And Security is selected
	And "Save" is "Disabled"
	When I select "Administrator" permission for server permission "Public"
	Then Server Permissions are "Visible" as
	| Windows Group          | Can Edit Windows Group | Deploy To | Deploy From | Administrator | View | Execute | Contribute | Delete Row | 
	| Warewolf Administrator | No                     | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   | 
	| Public                 | No                     | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   | 
	|                        | Yes                    |           |             |               |      |         |            |            | 
	Then Save is "Enabled"
	When I save the settings
	Then settings saved successfully
	And the validation message is ""


Scenario: Selecting Resource Permissions
	Given I have settings tab opened
	And selected server is "localhost"
	And "Save" is "Disabled"
	And Security is selected
	And Logging is not selected
	And Server Permissions are "Visible" as 
	| Windows Group          | Can Edit Windows Group | Deploy To | Deploy From | Administrator | View | Execute | Contribute | Delete Row |
	| Warewolf Administrator | No                     | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   |
	| Public                 | No                     | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   |
	|                        | Yes                    |           |             |               |      |         |            |            |
	
	When I add resource permission
	| Resource                        | Windows Group | View | Execute | Contribute |
	| WORKFLOWS\My Category\Dice Roll | Public        | Yes  | Yes     | Yes        |
	Then Resource Permissions are "Visible" as
	| Resource                        | Can Edit Resource | Windows Group | Can Edit Windows Group | View | Execute | Contribute |
	| WORKFLOWS\My Category\Dice Roll | Yes               | Public        | Yes                    | Yes  | Yes     | Yes        |
	|                                 | Yes               |               | Yes                    |      |         |            |
    Then Save is "Enabled"
	When I save the settings
	Then settings saved successfully
	And the validation message is ""


Scenario: Duplicate server permissions cannot be saved
	Given I have settings tab opened
	And selected server is "localhost"
	And "Save" is "Disabled"
	And Security is selected
	And Logging is not selected
	And Server Permissions are "Visible" as
	| Windows Group          | Can Edit Windows Group | Deploy To | Deploy From | Administrator | View | Execute | Contribute | Delete Row |
	| Warewolf Administrator | Yes                    | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   |
	| Public                 | Yes                    | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Enabled    |
	| Public                 | Yes                    | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Enabled    | 
    Then Save is "Enabled"
	When I save the settings
	Then settings not successfully saved
	And the validation message is "Duplicate server permission"

Scenario: Duplicate resource permissions cannot be saved
	Given I have settings tab opened
	And selected server is "localhost"
	And "Save" is "Disabled"
	And Security is selected
	And Logging is not selected
	When Resource Permissions are "Visible" as
	| Resources                       | Can Edit Resource | Windows Group | Can Edit Windows Group | View | Execute | Contribute |
	| WORKFLOWS\My Category\Dice Roll | Yes               | Public        | Yes                    | Yes  | Yes     | Yes        |
	| WORKFLOWS\My Category\Dice Roll | Yes               | Public        | Yes                    | Yes  | Yes     | Yes        |
    Then Save is "Enabled"
	When I save the settings
	Then settings not successfully saved
	And the validation message is "Duplicate resource permission"


Scenario: Selecting Logging is showing Server and Studio log settings
	Given I have settings tab opened
	And "server" selected as "localhost (Connected)"
	And Server edit is "Disabled"
	And server connection is "Disabled"
	And Save is "Disabled"
	And Security is "Selected"
	And Logging is "Unselected"
	And Server Permissions is "Visible"
	And Resource Permissions is "Visible"
	When I select "Logging"
	Then Server System Logs is "Visible"
	And Studio Logs is "Visible"
	And Server Permissions is "InVisible"
	And Resource Permissions is "InVisible"
	And Save is "Disabled"