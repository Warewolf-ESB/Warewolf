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
#Ensure delete button in the textbox is deleting variable in the variable textbox.
#Ensure removal from design surface updates list and list updates change list correctly
#Ensure bad variable names are in an error state
#Ensure unused variables do not appear in Debug Input window
#Ensure shorcut keys work

@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Variables adding in variable list and removing unused
	Given I have variables as
    | Variable    | Note              | Input | Output | IsUsed |
    | [[rec().a]] | This is recordset |       | YES    | YES    |
    | [[rec().b]] |                   |       |        |        |
    | [[mr()]]    |                   |       |        | YES    |
    | [[Var]]     |                   | YES   |        | YES    |
    | [[a]]       |                   |       |        |        |
    | [[lr().a]]  |                   |       |        |        |
	Then "Variables" is "Enabled"
	And variables filter box is "Visible"
	And "Filter Clear" is "Disabled"
	And "Delete Variables" is "Enabled"
	And "Sort Variables" is "Enabled" 
	And the Variable Names are
	| Variable Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| Var           |                  |                  | YES   |        |
	| a             | YES              |                  |       |        |
	And the Recordset Names are
	| Recordset Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| rec()          |                  |                  |       |        |
	| rec().a        |                  | YES              |       | YES    |
	| rec().b        | YES              |                  |       |        |
	| mr()           |                  |                  |       |        |
	| lr()           | YES              |                  |       |        |
	| lr().a         | YES              |                  |       |        |
	When I click "Delete Variables"
	And the Variable Names are
	| Variable Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| Var           |                  |                  | YES   |        |
	And the Recordset Names are
	| Recordset Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| mr()           |                  |                  |       |        |
	| rec()          |                  |                  |       |        |
	| rec().a        |                  | YES              |       | YES    |
						
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Sorting Variables in Variable list
	Given I have variables as
	 | Variable    | Note              | Input | Output | IsUsed |
	 | [[rec().a]] | This is recordset |        | YES     | YES    |
	 | [[rec().b]] |                   |        |         |        |
	 | [[mr()]]    |                   |        |         | YES    |
	 | [[Var]]     |                   | YES    |         | YES    |
	 | [[a]]       |                   |        |         |        |
	 | [[lr().a]]  |                   |        |         |        |
	When I Sort the variables
	And the Variable Names are
	| Variable Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| a             | YES              |                  |       |        |
	| Var           |                  |                  | YES   |        |
	And the Recordset Names are
	| Recordset Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| lr()           | YES              |                  |       |        |
	| lr().a         | YES              |                  |       |        |
	| mr()           |                  |                  |       |        |
	| rec()          |                  |                  |       |        |
	| rec().a        |                  | YES              |       | YES    |
	| rec().b        | YES              |                  |       |        |
	When I Sort the variables
	And the Variable Names are
	| Variable Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| Var           |                  |                  | YES   |        |
	| a             | YES              |                  |       |        |
	And the Recordset Names are
	| Recordset Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| rec()          |                  |                  |       |        |
	| rec().b        | YES              |                  |       |        |
	| rec().a        |                  | YES              |       | YES    |
	| mr()           |                  |                  |       |        |
	| lr()           | YES              |                  |       |        |
	| lr().a         | YES              |                  |       |        |
	
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Variable Errors
	Given the Variable Names are
	 | Variable | Error State | Delete IsEnabled | Error Tooltip                                     |
	 | a        |             |                  |                                                   |
	 | 1b       | YES         |                  | Variables must begin with alphabetical characters |
	 | b@       | YES         |                  | Variables contains invalid character              |
	 | b1       |             |                  |                                                   |
	And the Recordset Names are
	 | Recordset Name | Error State | Delete IsEnabled | Error Tooltip                                            |
	 | 1r()           | YES         |                  | Recordset names must begin with alphabetical characters  |
	 | 1r().a         |             |                  |                                                          |
	 | rec()          |             |                  |                                                          |
	 | rec().a        |             |                  |                                                          |
	 | rec().1a       | YES         |                  | Recordset fields must begin with alphabetical characters |
	 | rec().b        | YES         |                  | Duplicate Variable                                       |
	 | rec().b        | YES         |                  | Duplicate Variable                                       |
	
@MSTest:DeploymentItem:InfragisticsWPF4.Controls.Interactions.XamDialogWindow.v15.1.dll
@MSTest:DeploymentItem:Warewolf_Studio.exe
@MSTest:DeploymentItem:Newtonsoft.Json.dll
@MSTest:DeploymentItem:Microsoft.Practices.Prism.SharedInterfaces.dll
@MSTest:DeploymentItem:System.Windows.Interactivity.dll
Scenario: Variables removed from design surface and list
	Given I have variables as
    | Variable    | Note              | Input | Output | IsUsed |
    | [[rec().a]] | This is recordset |       | YES    | YES    |
    | [[rec().b]] |                   |       |        |        |
    | [[mr()]]    |                   |       |        |        |
    | [[Var]]     |                   | YES   |        | YES    |
    | [[a]]       |                   |       |        |        |
    | [[lr().a]]  |                   |       |        |        |
	And the Variable Names are
	| Variable Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| Var           |                  |                  | YES   |        |
	| a             | YES              |                  |       |        |
	And the Recordset Names are
	| Recordset Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| rec()          |                  |                  |       |        |
	| rec().a        |                  | YES              |       | YES    |
	| rec().b        | YES              |                  |       |        |
	| mr()           |                  |                  |       |        |
	| lr()           | YES              |                  |       |        |
	| lr().a         | YES              |                  |       |        |
	And I click delete for "[[a]]"
	And I click delete for "mr()"
	And the Variable Names are
	| Variable Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| a             | YES              |                  |       |        |
	And the Recordset Names are
	| Recordset Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| rec()          |                  |                  |       |        |
	| rec().a        |                  | YES              |       | YES    |
	| rec().b        | YES              |                  |       |        |
	| lr()           | YES              |                  |       |        |
	| lr().a         | YES              |                  |       |        |
	And I change variable Name from "a" to ""
	And I change Recordset Name from "rec()" to "this"
	And the Variable Names are
	| Variable Name | Delete IsEnabled | Note Highlighted | Input | Output |
	And the Recordset Names are
	| Recordset Name | Delete IsEnabled | Note Highlighted | Input | Output |
	| this()         | YES              |                  |       |        |
	| this().a       | YES              | YES              |       | YES    |
	| this().b       | YES              |                  |       |        |
	| lr()           | YES              |                  |       |        |
	| lr().a         | YES              |                  |       |        |
	| rec()          |                  |                  |       |        |
	| rec().a        |                  | YES              |       | YES    |