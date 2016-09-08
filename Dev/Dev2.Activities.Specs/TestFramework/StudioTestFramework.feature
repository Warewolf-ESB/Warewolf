﻿Feature: StudioTestFramework
	In order to test workflows in warewolf 
	As a user
	I want to create, edit, delete and update tests in a test window


Background: Setup for workflows for tests
			Given I have "Workflow 1" with inputs as
			| Input Var Name |
			| [[a]]              |
			And "Workflow 1" has outputs as
			| Ouput Var Name |
			| [[outputValue]]    |
			Given I have "Workflow 2" with inputs as
			| Input Var Name |
			| [[rec().a]]        |
			| [[rec().b]]        |
			And "Workflow 2" has outputs as
			| Ouput Var Name |
			| [[returnVal]]      |

Scenario: Create New Test
	Given the test builder is open with "Workflow 1"	
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	When I click New Test
	Then a new test is added
	#And Tab Header is "Workflow 1 - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is enabled
	And test status is pending
	And test is enabled

Scenario: Create New Test with Service that as recordset inputs
	Given the test builder is open with "Workflow 2"	
	And Tab Header is "Workflow 2 - Tests"
	And there are no tests
	When I click New Test
	Then a new test is added
	#And Tab Header is "Workflow 2 - Tests *"
	And test name starts with "Test 1"
	And username is blank
	And password is blank
	And inputs are
	| Variable Name | Value |
	| rec(1).a      |       |
	| rec(1).b      |       |
	When I updated the inputs as
	| Variable Name | Value |
	| rec(1).a      | val1  |
	| rec(1).b      |       |
	Then inputs are
	| Variable Name | Value |
	| rec(1).a      | val1  |
	| rec(1).b      |       |
	| rec(2).a      |       |
	| rec(2).b      |       |

Scenario: Save a New Test
	Given the test builder is open with "Workflow 1"
	And Tab Header is "Workflow 1 - Tests"
	And there are no tests
	And I click New Test
	Then a new test is added
	#And Tab Header is "Workflow 1 - Tests *"
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is enabled
	When I save
	Then Tab Header is "Workflow 1 - Tests"
	And I close the test builder
	When the test builder is open with "Workflow 1"
	Then Tab Header is "Workflow 1 - Tests"
	And there are 1 tests
	And "Test 1" is selected
	And test name starts with "Test 1"
	And inputs are
	| Variable Name | Value |
	| a             |       |
	And outputs as
	| Variable Name | Value |
	| outputValue   |       |
	And save is disabled
    



#
#Scenario: Edit existing test
#	Given test builder is open with "Workflow 1"
#	And I select "test1"
#	Then Test name is "test1"
#	And test URL is "http://localhost:3142/secure/Examples/Workflow 1.tests/test1"
#	And UserName is empty
#	And Password is empty
#	And Inputs are empty
#	And Outputs are empty
#	And No Error selected
#	And debug output is empty
#	And Tab name is "Workflow 1 - Tests"
#	And save is disabled
#	When I change the test name to "test2"
#	Then save is enabled
#	And Tab name is "Worklow 1 - Tests *"
#	When I save the test
#	Then test URL is "http://localhost:3142/secure/Examples/Workflow 1.tests/test2"
#	And Tab name is "Workflow 1 - Tests"
#	And save is disabled
#
#Scenario: Create new test from Explorer opens a new window with correct button state
#	Given I right click on the ExplorerItem on the explorer
#	Then The WorkflowTestBuilder window is opened
#	And The WorkflowTestBuilder has RunAll buttons disabled
#	And The WorkflowTestBuilder has Delete button disabled
#	And The WorkflowTestBuilder has Run button enabled
#
#
#Scenario: Create new test from Explorer opens a new window with correct test icon and state
#	Given I right click on the ExplorerItem on the explorer
#	Then The WorkflowTestBuilder window is opened
#	And The WorkflowTestBuilder has a pending status
#	And The WorkflowTestBuilder has a pending icon
#	And The test in the WorkflowTestBuilder is enabled
#
#
#Scenario: Create and Edit tests values sets the values for the test
#	Given I Change Test values on the WorkflowTestBuilder 
#	When I change the TestName to UnitTest1
#	Then the TestName is changed to UnitTest1
#	And The TestUrl is updated correctly
#	When The Workflow  UnitTest1 has "2" inputs
#	Then The Inputs variables are populated with "2" workflow inputs
#	When the Workflow has "1" OutPut 
#	Then the Outputs are populated with "1" inputs
#
#
#
#



	



