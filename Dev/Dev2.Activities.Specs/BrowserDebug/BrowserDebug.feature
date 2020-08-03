@ExecuteInBrowser
Feature: BrowserDebug
	In order to debug a workflow in Browser
	As a Warewolf user
	I want to be able to View full debug content in browser

Scenario: Executing an empty workflow
		Given I have a workflow "BlankWorkflow"
		When workflow "BlankWorkflow" is saved "1" time
		And I Debug "http://localhost:3142/secure/Acceptance%20Tests/BlankWorkflow.debug?" in Browser
		Then The Debug in Browser content contains "The workflow must have at least one service or activity connected to the Start Node."

Scenario: Executing a workflow with no inputs and outputs
		Given I have a workflow "NoInputsWorkflow"
		When workflow "NoInputsWorkflow" is saved "1" time
		And I Debug "http://localhost:3142/secure/Acceptance%20Tests/NoInputsWorkflow.debug?" in Browser
		Then The Debug in Browser content contains has children with no Inputs and Ouputs

Scenario: Executing Assign workflow with valid inputs
		Given I have a workflow "ValidAssignedVariableWF"
		And "ValidAssignedVariableWF" contains an Assign "ValidAssignVariables" as
			| variable      | value    |
			| [[dateMonth]] | February |
			| [[dateYear]]  | 2017     |
		When workflow "ValidAssignedVariableWF" is saved "1" time
		And I Debug "http://localhost:3142/secure/Acceptance%20Tests/ValidAssignedVariableWF.debug?" in Browser
		Then The Debug in Browser content contains has "2" inputs and "2" outputs for "ValidAssignVariables"

Scenario: Executing Assign workflow with invalid variable
		Given I have a workflow "InvalidAssignedVariableWF"
		And "InvalidAssignedVariableWF" contains an Assign "InvalidAssignVariables" as
			| variable  | value    |
			| d@teMonth | February |
		When workflow "InvalidAssignedVariableWF" is saved "1" time
		And I Debug "http://localhost:3142/secure/Acceptance%20Tests/InvalidAssignedVariableWF.debug?" in Browser
		Then The Debug in Browser content contains has error messagge ""invalid variable assigned to d@teMonth""

Scenario: Executing Hello World workflow
		Given I have a workflow "Hello World"
		And I Debug "http://localhost:3142/secure/Hello%20World.debug?Name=Bob" in Browser
		Then The Debug in Browser content contains has "3" inputs and "1" outputs for "Decision"
		Then The Debug in Browser content contains has "1" inputs and "1" outputs for "Set the output variable (1)"

Scenario: Executing Hello World workflow with no Name Input
		Given I have a workflow "Hello World"
		And I Debug "http://localhost:3142/secure/Hello%20World.debug?Name=" in Browser
		Then The Debug in Browser content contains has "3" inputs and "1" outputs for "Decision"
		Then The Debug in Browser content contains has "1" inputs and "1" outputs for "Set the output variable (1)"

Scenario: Executing a Sequence workflow
		Given I have a workflow "SequenceVariableWF"
		And "SequenceVariableWF" contains a Sequence "SequenceFlow" as
		And "SequenceFlow" contains an Assign "AssignFlow" as
			| variable      | value    |
			| [[dateMonth]] | February |
			| [[dateDay]]	| Thursday |
		And "SequenceFlow" contains case convert "CaseConvertFlow" as
			| Variable  | Type  |
			| [[dateMonth]] | UPPER |
			| [[dateDay]]	| UPPER |
		And "SequenceFlow" contains Replace "ReplaceFlow" into "[[replaceResult]]" as	
			| In Fields | Find | Replace With |
			| [[dateDay]] | THURSDAY    | Friday      |
		When workflow "SequenceVariableWF" is saved "1" time
		And I Debug "http://localhost:3142/secure/Acceptance%20Tests/SequenceVariableWF.debug?" in Browser
		Then The Debug in Browser content contains order of "AssignFlow", "CaseConvertFlow" and "ReplaceFlow" in SequenceFlow

Scenario: Executing a Foreach workflow
		Given I have a workflow "ForEachAssigneWF"
		And "ForEachAssigneWF" contains a Foreach "ForEachTest" as "NumOfExecution" executions "4"
		And "ForEachTest" contains an Assign "MyAssign" as
	    | variable  | value |
	    | [[Year]]	| 2017  |
		When workflow "ForEachAssigneWF" is saved "1" time
		And I Debug "http://localhost:3142/secure/Acceptance%20Tests/ForEachAssigneWF.debug?" in Browser
	    And The 1 debug state has 4 children
  
Scenario: Executing a Dotnet plugin workflow
		Given I have a workflow "DotNetDLLWf"
		And "DotNetDLLWf" contains an DotNet DLL "DotNetService" as
	     | Source                   | ClassName                       | ObjectName | Action    | ActionOutputVaribale |
	     | New DotNet Plugin Source | TestingDotnetDllCascading.Human | [[@human]] | BuildInts | [[rec1().num]]       |		 
		And "DotNetService" constructorinputs 0 with inputs as
		| parameterName | value |type|	  
		And "DotNetService" service Action "BuildInts" with inputs and output "[[rec1().num]]" as 
		| parameterName | value | type         |
		| a             | 1     | System.Int32 |
		| b             | 1     | System.Int32 |
		| c             | 1     | System.Int32 |
		| d             | 1     | System.Int32 |
		When workflow "DotNetDLLWf" is saved "1" time
		And I Debug "http://localhost:3142/secure/Acceptance%20Tests/DotNetDLLWf.debug?" in Browser
		And The Debug in Browser content contains for Dotnet has 3 states
		And The 1 debug state has 2 children
		And The 0 debug state has 0 children
		And The 2 debug state has 0 children
		 

Scenario: Executing a Forward Sort Recordset workflow
		Given I have a workflow "SortRecordsetWF"
		And "SortRecordsetWF" contains an Assign "ExampleRecordSet" as
			|			variable			|	value	|
			|	[[Degree(1).YearCompleted]]	|	2015	|
			|	[[Degree(2).YearCompleted]]	|	2012	|
			|	[[Degree(3).YearCompleted]]	|	2014	|
			|	[[Degree(4).YearCompleted]]	|	2013	|
		And "SortRecordsetWF" contains an Sort "Degree" as
			| Sort Field  | Sort Order |
			| [[Degree().YearCompleted]] | Forward  |
		And workflow "SortRecordsetWF" is saved "1" time
		And I Debug "http://localhost:3142/secure/Acceptance%20Tests/SortRecordsetWF.debug?" in Browser
		Then Debugstate in index 2 has output as 
			| Values |
			| 2012   |
			| 2013   |
			| 2014   |
			| 2015   |

Scenario: Executing Hello world in browser 
	Given I Debug "http://localhost:3142/secure/Hello%20World.json?Name=&wid=5f895e8d-07a3-4f87-869f-7c03d86f330b" in Browser
	Then Browser content is "Hello World."

Scenario: Executing Workflow with empty Json Assign in browser 
	Given I Debug "http://localhost:3142/secure/AssignOnlyWithNoOutput.json" in Browser
	Then Browser content is "{}"

Scenario: Executing Workflow with empty Xml Assign in browser 
	Given I Debug "http://localhost:3142/secure/AssignOnlyWithNoOutput.xml" in Browser
	Then Browser content is "<DataList />"

Scenario: Executing Workflow with Execute Permissions and Nested Workflow With No Execute Permissions
	Given I have a workflow "OuterWorkflow"
	And Public "Has" Permissions to Execute "OuterWorkflow"
	And I Debug "http://localhost:3142/public/OuterWorkflow.json?" in Browser
	Then Browser content is "Access has been denied for this request."

Scenario: Executing Workflow with No Execute Permissions
	Given I have a workflow "Nested"
	And Public "" Permissions to Execute "Nested"
	And I Debug "http://localhost:3142/public/Nested.json?" in Browser
	Then Browser content is "Access has been denied for this request."

Scenario: Executing a workflow should not error for logging
		Given I have a workflow "AssignedWF"
		And "AssignedWF" contains an Assign "AssignVar" as
			| variable      | value    |
			| [[dateMonth]] | February |
		When workflow "AssignedWF" is saved "1" time
		And I Execute "http://localhost:3142/secure/Acceptance%20Tests/AssignedWF.json" in Browser
		Then Browser content is not "FatalError"

Scenario: Executing a workflow always returns outputs even when error
		Given I have a workflow "ErrorWebResponse"
		And I Debug "http://localhost:3142/secure/ErrorWebResponse.json" in Browser		
		Then Browser content is ""Message":"
		Then Browser content is not "FatalError"
	