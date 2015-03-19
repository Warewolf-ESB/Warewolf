@VariableList
Feature: VariableList
	In order to manage my variables
	As a Warewolf user
	I want to be told shown all variables in my workflow service


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


Scenario: Variables adding in variable list 
	Given I have variables as
    | Variable    | Note              | Input | Output | Not Used |
    | [[rec().a]] | This is recordset |       |        |          |
    | [[rec().a]] | This is recordset |       |        |          |
    | [[mr().a]]  |                   |       |        |          |
    | [[mr().a]]  |                   |       |        |          |
    | [[Var]]     |                   |       |        |          |
    | [[a]]       |                   |       |        | Yes      |
    | [[lr().a]]  |                   |       |        | Yes      |
	Then "Variables" is "Enabled"
	And variables filter box is "Visible"
	And "Filter Clear" is "Disabled"
	And "Delete Variables" is "Disabled"
	And "Sort Variables" is "Enabled" 
	And the Variable Names are
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| Var           | NO             | Yes          | No               |       |        |
	|               | NO             | NO           | NO               |       |        |
	And the Recordset Names are
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| rec()          | NO             | Yes          | Yes              |       |        |
	| rec().a        | NO             | Yes          | Yes              |       |        |
	| mr()           | NO             | Yes          |                  |       |        |
	| mr().a         | NO             | Yes          |                  |       |        |
	|                | No             | No           |                  |       |        |  
								
Scenario: Deleting Unassigned Variables on variable list
	Given I have variables as
	 | Variable    | Note              | Inputs | Outputs | Not Used |
	 | [[rec().a]] | This is recordset |        |         |          |
	 | [[rec().a]] | This is recordset |        |         |          |
	 | [[mr().a]]  |                   |        |         |          |
	 | [[mr().a]]  |                   |        |         |          |
	 | [[Var]]     |                   |        |         |          |
	 | [[a]]       |                   |        |         | Yes      |
	 | [[lr().a]]  |                   |        |         | Yes      |
	Then "Variables" is "Enabled"
	And variables filter box is "Visible"
	And "Filter Clear" is "Disabled"
	And "Delete Variables" is "Enabled"
	And "Sort Variables" is "Enabled" 
	And the Variable Names are
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| Var           | NO             | Yes          | No               |       |        |
	| [[a]]         | Yes            | NO           | NO               |       |        |
	|               | NO             | NO           | NO               |       |        |
	And the Recordset Names are
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| rec()          | NO             | Yes          | Yes              |       |        |
	| rec().a        | NO             | Yes          | Yes              |       |        |
	| mr()           | NO             | Yes          | No               |       |        |
	| mr().a         | NO             | Yes          | No               |       |        |
	| [[lr().a]]     | Yes            | NO           | No               |       |        |
	|                | NO             | NO           | NO               |       |        | 
	When I delete unassigned variables
	Then the Variable Names are
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| Var           | Yes            | Yes          | No               |             |             |
	|               | NO             | NO           | NO               |       |        |
	And the Recordset Names are 
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input | Output |
	| rec()          | Yes            | Yes          | No               |       |        |
	| rec().a        | Yes            | Yes          | Yes              |       |        |
	| mr()           | Yes            | Yes          | No               |       |        |
	| mr().a         | Yes            | Yes          | No               |       |        |
	|                | NO             | NO           | NO               |       |        |

Scenario: Searching Variables in Variable list
	Given I have variables as
	 | Variable    | Note              | Inputs | Outputs | Not Used |
	 | [[rec().a]] | This is recordset |        |         |          |
	 | [[rec().a]] | This is recordset |        |         |          |
	 | [[mr().a]]  |                   |        |         |          |
	 | [[mr().a]]  |                   |        |         |          |
	 | [[Var]]     |                   |        |         |          |
	 | [[a]]       |                   |        |         | Yes      |
	 | [[lr().a]]  |                   |        |         | Yes      |
	Then "Variables" is "Enabled"
	And variables filter box is "Visible"
	And "Filter Clear" is "Disabled"
	And "Delete Variables" is "Disabled"
	And "Sort Variables" is "Enabled" 
	When I search for variable "[[mr().a]]"
	Then the Variable Names are
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	And the Recordset Names are 
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| mr()           | Yes            | Yes          | No               |             |             |
	| mr().a         | Yes            | Yes          | No               |             |             |
	When I clear the filter 
	Then the Variable Names are
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| Var           | Yes            | Yes          | No               |             |             |
	| [[a]]         | NO             | NO           | NO               | Not Visible | Not Visible |
	|               | NO             | NO           | NO               |             |             |
	And the Recordset Names are
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| rec()          | Yes            | Yes          | No               |             |             |
	| rec().a        | Yes            | Yes          | Yes              |             |             |
	| mr()           | Yes            | Yes          | No               |             |             |
	| mr().a         | Yes            | Yes          | No               |             |             |
	| [[lr().a]]     | Yes            | No           | No               | Not Visible | Not Visible |
	|                | NO             | NO           | NO               |             |             |  



Scenario: Sorting Variables in Variable list
	Given I have variables as
	 | Variable    | Note              | Inputs | Outputs | Not Used |
	 | [[rec().a]] | This is recordset |        |         |          |
	 | [[rec().a]] | This is recordset |        |         |          |
	 | [[mr().a]]  |                   |        |         |          |
	 | [[mr().a]]  |                   |        |         |          |
	 | [[Var]]     |                   |        |         |          |
	 | [[a]]       |                   |        |         | Yes      |
	 | [[lr().a]]  |                   |        |         | Yes      |
	Then "Variables" is "Enabled"
	And variables filter box is "Visible"
	And "Filter Clear" is "Disabled"
	And "Delete Variables" is "Disabled"
	And "Sort Variables" is "Enabled" 
	When I Sort the variables 
	Then the Variable Names are
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| [[a]]         | NO             | NO           | NO               | Not Visible | Not Visible |
	| Var           | Yes            | Yes          | No               |             |             |
	|                | NO             | NO           | NO               |             |             |  
	And the Recordset Names are
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| [[lr().a]]     | Yes            | No           | No               | Not Visible | Not Visible |
	| mr()           | Yes            | Yes          | No               |             |             |
	| mr().a         | Yes            | Yes          | No               |             |             |
	| rec()          | Yes            | Yes          | No               |             |             |
	| rec().a        | Yes            | Yes          | Yes              |             |             |
	|                | NO             | NO           | NO               |             |             |  
	When I Sort the variables 
	Then the Variable Names are
	| Variable Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| Var           | Yes            | Yes          | No               |             |             |
	| [[a]]         | NO             | NO           | NO               | Not Visible | Not Visible |
	|                | NO             | NO           | NO               |             |             |  
	And the Recordset Names are
	| Recordset Name | Delete Visible | Note Visible | Note Highlighted | Input       | Output      |
	| rec()          | Yes            | Yes          | No               |             |             |
	| rec().a        | Yes            | Yes          | Yes              |             |             |
	| mr()           | Yes            | Yes          | No               |             |             |
	| mr().a         | Yes            | Yes          | No               |             |             |
	| [[lr().a]]     | Yes            | No           | No               | Not Visible | Not Visible |
	|                | NO             | NO           | NO               |             |             |  