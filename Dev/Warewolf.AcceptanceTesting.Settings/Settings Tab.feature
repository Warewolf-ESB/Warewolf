Feature: Settings Tab
	In order to manage various server settingsss
	As a Warewolf User
	I want to be shown and allowed to edit the server settings

##Default Security Settings
##Selecting Admin rights for public
##Selecting Resource Permissions
##Duplicate server permissions cannot be saved
##Duplicate resource permissions cannot be saved
##Save enables when I change server logs
##Save enables when I change studio logs
##Log File hyper link is opening log file
##Studio Log File hyper link is opening log file
##Server and studio defaults file size 
##Server and studio log file size only accepts numbers
##Load remote Server log file


@Settings
Scenario: Default Security Settings
	Given I have settings tab opened
	And selected server is "localhost"
	And Security is selected
	And Logging is not selected
	And Server Permissions are "Visible" as
	| Windows Group          | Can Edit Windows Group | Deploy To | Deploy From | Administrator | View | Execute | Contribute | Delete Row |
	| Warewolf Administrator | No                     | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   |
	| Public                 | No                     | No        | No          | No            | No   | No      | No         | Disabled   |
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
	| Public                 | Yes                    | Yes       | Yes         | Yes           | Yes  | Yes     | Yes        | Disabled   |
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

Scenario Outline: Save enables when I change server logs
	Given I have settings tab opened	
	And Logging is selected
	Then Server System Logs is "Visible"
	And Studio Logs is "Visible"
	When Server System Logs setup as "<Serverlogs>"
	And Save is "Enabled"
Examples: 
     | No | Serverlogs |
     | 1  | None       |
     | 2  | Fatal      |
     | 3  | Error      |
     | 4  | Warn       |
     | 6  | Info       |
     | 7  | Trace      |
     

Scenario Outline: Save enables when I change studio logs
	Given I have settings tab opened	
	And Logging is selected
	Then Server System Logs is "Visible"
	And Studio Logs is "Visible"
	When Studio System Logs selected "<Studiologs>"
	And Save is "Enabled"
Examples: 
     | No | Studiologs |
     | 1  | None       |
     | 2  | Fatal      |
     | 3  | Error      |
     | 4  | Warn       |
     | 6  | Info       |
     | 7  | Trace      |

	
Scenario: Server Log File hyper link is opening log file
	Given I have settings tab opened	
	And Logging is selected
	Then Server System Logs is "Visible"
	And Studio Logs is "Visible"
	And Server Log File hyper link is "Visible"
	And Studio Log File hyper link is "Visible"
	When I click "Server Log File"
	Then "Localhost server Log.txt - Notepad" is opened
	
	
Scenario: Studio Log File hyper link is opening log file
	Given I have settings tab opened	
	And Logging is selected
	Then Server System Logs is "Visible"
	And Studio Logs is "Visible"
	And Server Log File hyper link is "Visible"
	And Studio Log File hyper link is "Visible"
	When I click "Studio Log File"
	Then "Warewolf Studio.log - Notepad" is opened	
	
	
Scenario: Server and studio defaults file size 
	Given I have settings tab opened for the first time	
	And Logging is selected
	Then Server System Logs is "Visible"
	And Studio Logs is "Visible"	
	And Server System Logs selected "Warning"
	And Studio System Logs selected "Warning"
	And Max Log file Size for Server default is "200" MB
	And Max Log file Size for Studio default is "200" MB


Scenario: Server and studio log file size only accepts numbers
	Given I have settings tab opened	
	And Logging is selected
	Then Server System Logs is "Visible"
	And Studio Logs is "Visible"	
	When I edit Max Log file Size for Server "abc" MB
	Then Max Log file Size for Server default is "200" MB 
	When I edit Max Log file Size for Studio "abc" MB
	Then Max Log file Size for Studio default is "200" MB
	And "Save" is "Disabled"
	When I edit Max Log file Size for Server "100" MB
	Then Max Log file Size for Server default is "100" MB 
	And "Save" is "Enabled"

Scenario: Load remote Server log file
	Given I have settings tab opened
	And Logging is selected
	And I connect to "RemoteServer"
	And remote server has
	| ServerLog | ServerSize |
	| None      | 1          |
	Then Max Log file Size for Server is "1" MB
	And Server System Logs selected "None"
	And Studio System Logs is "Disabled"
	And Max Log file Size for Studio is "Disabled"
	And Studio Log File Link is "Disabled"
	When Studio System Logs selected "Info"
	Then "Save" is "Enabled"