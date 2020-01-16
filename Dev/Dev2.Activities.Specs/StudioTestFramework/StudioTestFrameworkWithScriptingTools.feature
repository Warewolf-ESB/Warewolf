@StudioTestFrameworkWithScriptingTools
Feature: StudioTestFrameworkWithScriptingTools
	In order to test workflows that contain scripting tools in warewolf 
	As a user
	I want to create, edit, delete and update tests in a test window


Background: Setup for workflows for tests
		Given test folder is cleaned	
		And I have "Workflow 1" with inputs as
			| Input Var Name |
			| [[a]]          |
		And "Workflow 1" has outputs as
			| Ouput Var Name  |
			| [[outputValue]] |		
		Given I have "Workflow 2" with inputs as
			| Input Var Name |
			| [[rec().a]]    |
			| [[rec().b]]    |
		And "Workflow 2" has outputs as
			| Ouput Var Name |
			| [[returnVal]]  |
		Given I have "Workflow 3" with inputs as
			| Input Var Name |
			| [[A]]              |
			| [[B]]              |
			| [[C]]              |
		And "Workflow 3" has outputs as
			| Ouput Var Name |
			| [[message]]    |
		Given I have "WorkflowWithTests" with inputs as 
			| Input Var Name |
			| [[input]]      |
		And "WorkflowWithTests" has outputs as
			| Ouput Var Name  |
			| [[outputValue]] |			
		And "WorkflowWithTests" Tests as 
			| TestName | AuthenticationType | Error | TestFailing | TestPending | TestInvalid | TestPassed |
			| Test1    | Windows            | false | false       | false       | false       | true       |
			| Test2    | Windows            | false | true        | false       | false       | false      |
			| Test3    | Windows            | false | false       | false       | true        | false      |
			| Test4    | Windows            | false | false       | true        | false       | false      |
			

Scenario: Test WF with Cmd Script
	Given I have a workflow "CmdScriptTestWF"	
	And "CmdScriptTestWF" contains a Cmd Script "testCmdScript" ScriptToRun "echo Kingdom of KwaZulu Natal" and result as "[[result]]"	
	And I save workflow "CmdScriptTestWF"
	Then the test builder is open with "CmdScriptTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "testCmdScript" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value                    |
	  	 | [[result]]    | =         | Kingdom of KwaZulu Natal |  
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "CmdScriptTestWF" is deleted as cleanup
	
Scenario: Test WF with JavaScript
	Given I have a workflow "JavaScriptTestWF"	
	And "JavaScriptTestWF" contains a Java Script "testJavaScript" ScriptToRun "return Math.sqrt(49);" and result as "[[result]]"
	And I save workflow "JavaScriptTestWF"
	Then the test builder is open with "JavaScriptTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "testJavaScript" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value |
	  	 | [[result]]    | =         | 7     |  
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "JavaScriptTestWF" is deleted as cleanup

Scenario: Test WF with Python
	Given I have a workflow "PythonTestWF"	
	And "PythonTestWF" contains a Python "testPython" ScriptToRun "return { '1': "one", '2': "two",}.get('7', "not one or two")" and result as "[[result]]"		
	And I save workflow "PythonTestWF"
	Then the test builder is open with "PythonTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "testPython" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value          |
	  	 | [[result]]    | =         | not one or two |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "PythonTestWF" is deleted as cleanup

Scenario: Test WF with Ruby
	Given I have a workflow "RubyTestWF"
	And "RubyTestWF" contains a Ruby "testRuby" ScriptToRun "sleep(5)" and result as "[[result]]"		
	And I save workflow "RubyTestWF"
	Then the test builder is open with "RubyTestWF"
	And I click New Test
	And a new test is added	
    And test name starts with "Test 1"
	And I Add "testRuby" as TestStep
	And I add StepOutputs as  
	  	 | Variable Name | Condition | Value |
	  	 | [[result]]    | =         | 5     |
	When I save
	And I run the test
	Then test result is Passed
	When I delete "Test 1"
	Then The "DeleteConfirmation" popup is shown I click Ok
	Then workflow "RubyTestWF" is deleted as cleanup
