Feature: VariableList
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers


## System Requirements for Variable List
#Ensure variables used in the tools are adding automatically to variable list.
#Ensure user is able search for variable in variable list.
#Ensure search clear button is clearing text in variable list search box.
#Ensure user is able to Delete all the unused variables in variable list.
#Ensure sort alphabetically button is available in variable list box.
#Ensure scalar variables are Sorting alphabetically when user clicks on sort button.
#Ensure Recordset variables are Sorting alphabetically when user clicks on sort button.
#Ensure user is able to select variables as input.
#Ensure user is able to select variables as output.
#Ensure input and output checkboxes must be disabled if variable is unused.
#Ensure input and Output checkbox is disabled when variable in the variable box is unused.
#Ensure variable list has separate groups for recordset and scalar variable.
#Ensure variable textbox has delete and note button disabled.
#Ensure delete button in textbox is highlighted when variable is unused.
#Ensure delete button in the textbox is deleting variable in the variable textbox.
#Ensure variable Notes button is available in the variable box.
#Ensure Notes button is disabled if the variable box is empty.
#Ensure note textbox is opened when user click on button.
#Ensure user is able to edit notes of the variable in variable note box.
#Ensure one scrollbar is made for variable list to move up and down.
#Ensure next variable textboxe appears only when user has a variable in previous box.





@VariableList
Scenario: Variables adding in variable list 
	Given I open workflow "VariableTest1"
	And "VariableTest1" contains
	| Variable        | Value              |
	| [[rec().a]] = 1 | 1 |        
	| [[rec().a]] = 2 | This is recordset                  |        
	| [[mr().a]] =    |                   |        |         |            |
	| [[mr().a]] =    |                   |        |         |            |
	| [[Var]] = ball  |                   |        |         |            |
	| [[a]]           |                   |        |         | Yes        |
	| [[lr().a]]      |                   |        |         | Yes        |
	Then Variables box is "Enabled"
	And variables filter box is "Visible"
	And Filter Clear button is "Disabled"
	And Delete  unassigned variables button is "Disabled"
	And Sort variables order button is "Enabled" 
	And the Variables Names look like 
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| Var           | Yes            | Yes          | No               |       |        |
	|               | NO             | NO           | NO               |       |        |
	And the Recordset Name look like 
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| rec()          | Yes            | Yes          |                  |       |        |  
	| rec().a        | Yes            | Yes          |                  |       |        |  
	| mr()           | Yes            | Yes          |                  |       |        |  
	| mr().a         | Yes            | Yes          |                  |       |        |  
	|                | No             | No           |                  |       |        |  
								


Scenario: Deleting Unassigned Variables on variable list
	Given I open workflow "VariableTest2"
	And "VariableTest" variable contains
	| Variable        | Note              | Inputs | Outputs | Unassigned |
	| [[rec().a]] = 1 | This is recordset |        |         |            |
	| [[rec().a]] = 2 |                   |        |         |            |
	| [[mr().a]] =    |                   |        |         |            |
	| [[mr().a]] =    |                   |        |         |            |
	| [[Var]] = ball  | This is scalar    |        |         |            |
	| [[a]]           |                   |        |         | Yes        |
	| [[lr().a]]      |                   |        |         | Yes        |
	Then Variables box is "Enabled"
	And variables filter box is "Visible"
	And Filter Clear button is "Disabled"
	And Delete  unassigned variables button is "Enabled"
	And Sort variables order button is "Enabled" 
	And the Variables Names look like 
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| Var           | Yes            | Yes          | No               |             |             |
	| [[a]]         | NO             | NO           | NO               | Not Visible | Not Visible |
	And the Recordset Name look like 
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| rec()          | Yes            | Yes          | No               |             |             |
	| rec().a        | Yes            | Yes          | Yes              |             |             |
	| mr()           | Yes            | Yes          | No               |             |             |
	| mr().a         | Yes            | Yes          | No               |             |             |
	| [[lr().a]]     | Yes            | No           | No               | Not Visible | Not Visible |  
	When I delete unassigned variables
	Then the Variable Names look like
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| Var           | Yes            | Yes          | No               |             |             |
	And the Recordset Name look like 
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| rec()          | Yes            | Yes          | No               |             |             |
	| rec().a        | Yes            | Yes          | Yes              |             |             |
	| mr()           | Yes            | Yes          | No               |             |             |
	| mr().a         | Yes            | Yes          | No               |             |             |



Scenario: Searching Variables in Variable list
	Given I open workflow "VariableTest1"
	And "VariableTest" variable contains
	| Variable        | Note              | Inputs | Outputs | Unassigned |
	| [[rec().a]] = 1 | This is recordset |        |         |            |
	| [[rec().a]] = 2 |                   |        |         |            |
	| [[mr().a]] =    |                   |        |         |            |
	| [[mr().a]] =    |                   |        |         |            |
	| [[Var]] = ball  |                   |        |         |            |
	| [[a]]           |                   |        |         | Yes        |
	| [[lr().a]]      |                   |        |         | Yes        |
	Then Variables box is "Enabled"
	And variables filter box is "Visible"
	And Filter Clear button is "Disabled"
	And Delete  unassigned variables button is "Disabled"
	And Sort variables order button is "Enabled" 
	When I search for variable "[[mr().a]]"
	Then the Variables Names look like 
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	And the Recordset Name look like 
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| mr()           | Yes            | Yes          | No               |             |             |
	| mr().a         | Yes            | Yes          | No               |             |             |
	When I clear the filter 
	Then the Variables Names look like 
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| Var           | Yes            | Yes          | No               |             |             |
	| [[a]]         | NO             | NO           | NO               | Not Visible | Not Visible |
	And the Recordset Name look like 
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| rec()          | Yes            | Yes          | No               |             |             |
	| rec().a        | Yes            | Yes          | Yes              |             |             |
	| mr()           | Yes            | Yes          | No               |             |             |
	| mr().a         | Yes            | Yes          | No               |             |             |
	| [[lr().a]]     | Yes            | No           | No               | Not Visible | Not Visible |  



Scenario: Sorting Variables in Variable list
	Given I open workflow "VariableTest1"
	And "VariableTest" variable contains

















	| Variable        | Note              | Inputs | Outputs | Unassigned |
	| [[rec().a]] = 1 | This is recordset |        |         |            |
	| [[rec().a]] = 2 |                   |        |         |            |
	| [[mr().a]] =    |                   |        |         |            |
	| [[mr().a]] =    |                   |        |         |            |
	| [[Var]] = ball  |                   |        |         |            |
	| [[a]]           |                   |        |         | Yes        |
	| [[lr().a]]      |                   |        |         | Yes        |
	Then Variables box is "Enabled"
	And variables filter box is "Visible"
	And Filter Clear button is "Disabled"
	And Delete  unassigned variables button is "Disabled"
	And Sort variables order button is "Enabled" 
	When I Sort the variables 
	Then the Variables Names look like 
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| [[a]]         | NO             | NO           | NO               | Not Visible | Not Visible |
	| Var           | Yes            | Yes          | No               |             |             |
	And the Recordset Name look like 
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| [[lr().a]]     | Yes            | No           | No               | Not Visible | Not Visible |
	| mr()           | Yes            | Yes          | No               |             |             |
	| mr().a         | Yes            | Yes          | No               |             |             |
	| rec()          | Yes            | Yes          | No               |             |             |
	| rec().a        | Yes            | Yes          | Yes              |             |             |
	When I Sort the variables 
	Then the Variables Names look like 
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| Var           | Yes            | Yes          | No               |             |             |
	| [[a]]         | NO             | NO           | NO               | Not Visible | Not Visible |
	And the Recordset Name look like 
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| rec()          | Yes            | Yes          | No               |             |             |
	| rec().a        | Yes            | Yes          | Yes              |             |             |
	| mr()           | Yes            | Yes          | No               |             |             |
	| mr().a         | Yes            | Yes          | No               |             |             |
	| [[lr().a]]     | Yes            | No           | No               | Not Visible | Not Visible |



Scenario: Variablebox is enabled for design surface
	Given I have "New Workflow Service"
	Then Variables box is "Enabled"

Scenario: Variablebox is Disabled for Server Source
	Given I have "Server Source" tab
	Then Variables box is "Disabled"

Scenario: Variablebox is Disabled for Database Service
	Given I have "Database Service" tab
	Then Variables box is "Disabled"

Scenario: Variablebox is Disabled for Database Source
	Given I have "Database Source" tab
	Then Variables box is "Disabled"


Scenario: Variablebox is Disabled for New Plugin Service
	Given I have "New Plugin Service" tab
	Then Variables box is "Disabled"












